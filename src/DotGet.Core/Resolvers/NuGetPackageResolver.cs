using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;

using DotGet.Core.Configuration;

using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Packaging;
using NuGet.Packaging.Core;

namespace DotGet.Core.Resolvers
{
    internal class NuGetPackageResolver : Resolver
    {
        private string NuGetFeed;
        private string NuGetPackagesRoot;

        public NuGetPackageResolver(string source, ResolverOptions options) : base(source, options)
        {
            bool customNuGetFeed = Options.TryGetValue("feed", out NuGetFeed);
            NuGetFeed = customNuGetFeed ? NuGetFeed : "https://api.nuget.org/v3/index.json";
            NuGetPackagesRoot = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Environment.GetEnvironmentVariable("USERPROFILE") : Environment.GetEnvironmentVariable("HOME");
            NuGetPackagesRoot = Path.Combine(NuGetPackagesRoot, ".nuget", "packages");
        }

        public override bool CanResolve() => !Source.Contains("/") && !Source.Contains(@"\") && !Source.StartsWith(".");

        public override (bool, string) Resolve()
        {
            IPackageSearchMetadata package = GetPackageFromFeed(Source);
            if (package == null)
                return (false, "Package not found!");

            bool isNetCoreApp = HasNetCoreAppDependencyGroup(package);
            if (!isNetCoreApp)
                return (false, "Package does not support .NETCoreApp!");

            try
            {
                InstallNuGetPackage(package.Identity.Id, package.Identity.Version.ToFullString());
            }
            catch (System.Exception ex)
            {
                return (false, ex.Message);
            }

            string netcoreappDirectory = package.DependencySets.Select(d => d.TargetFramework).LastOrDefault(t => t.Framework == ".NETCoreApp").GetShortFolderName();
            string dllDirectory = Path.Combine(NuGetPackagesRoot, BuildPackageDirectoryPath(package.Identity.Id, package.Identity.Version.ToFullString()), "lib", netcoreappDirectory);

            DirectoryInfo directoryInfo = new DirectoryInfo(dllDirectory);
            FileInfo dll = directoryInfo.GetFiles().FirstOrDefault(f => f.Extension == ".dll");
            if (dll == null)
                return (false, "No executable found in package!");

            return (true, dll.FullName);
        }

        private bool HasNetCoreAppDependencyGroup(IPackageSearchMetadata package)
            => package.DependencySets.Where(d => d.TargetFramework.Framework == ".NETCoreApp").Count() > 0;

        private IPackageSearchMetadata GetPackageFromFeed(string packageId, string version = "")
        {
            List<Lazy<INuGetResourceProvider>> providers = new List<Lazy<INuGetResourceProvider>>();
            providers.AddRange(Repository.Provider.GetCoreV3());

            PackageSource packageSource = new PackageSource(NuGetFeed);
            SourceRepository sourceRepository = new SourceRepository(packageSource, providers);
            PackageMetadataResource packageMetadataResource = sourceRepository.GetResource<PackageMetadataResource>();
            IEnumerable<IPackageSearchMetadata> searchMetadata = packageMetadataResource
                .GetMetadataAsync(packageId, true, true, new Logger(), CancellationToken.None).Result;

            return version == string.Empty
                ? searchMetadata.LastOrDefault() : searchMetadata.FirstOrDefault(s => s.Identity.Version.ToFullString() == version);
        }

        private void InstallNuGetPackage(string packageId, string version)
        {
            IPackageSearchMetadata package = GetPackageFromFeed(packageId, version);
            if (IsPackageInstalled(packageId, version))
                return;

            DownloadNuGetPackage(packageId, version);
            foreach (PackageDependencyGroup dependencySet in package.DependencySets)
            {
                foreach (PackageDependency pkg in dependencySet.Packages)
                    InstallNuGetPackage(pkg.Id, pkg.VersionRange.MinVersion.ToFullString());
            }
        }

        private void DownloadNuGetPackage(string packageId, string version)
        {
            Logger logger = new Logger();

            string fullName = BuildPackageDirectoryPath(packageId, version) + ".nupkg";
            string requestUri = "https://api.nuget.org/packages/" + fullName;
            logger.LogSummary("  GET " + requestUri);

            HttpClient client = new HttpClient();
            byte[] bytes = client.GetByteArrayAsync(requestUri).Result;
            logger.LogSummary("  OK " + requestUri);

            File.WriteAllBytes(Path.Combine(NuGetPackagesRoot, fullName), bytes);
            UnpackNuGetPackage(packageId, version);
        }

        private void UnpackNuGetPackage(string packageId, string version)
        {
            string fullName = BuildPackageDirectoryPath(packageId, version) + ".nupkg";
            string fullPath = Path.Combine(NuGetPackagesRoot, fullName);
            string unzipPath = Path.Combine(NuGetPackagesRoot, packageId.ToLower(), version);

            ZipFile.ExtractToDirectory(fullPath, unzipPath);
            File.Move(fullPath, unzipPath + "/" + fullName);
        }

        private bool IsPackageInstalled(string packageId, string version)
            => Directory.Exists(Path.Combine(NuGetPackagesRoot, BuildPackageDirectoryPath(packageId, version)));

        private string BuildPackageDirectoryPath(string packageId, string version) => Path.Combine(packageId.ToLower(), version);

        private class Logger : ILogger
        {
            public void LogDebug(string data) => Console.WriteLine(data);
            public void LogVerbose(string data) => Console.WriteLine(data);
            public void LogInformation(string data) => Console.WriteLine(data);
            public void LogMinimal(string data) => Console.WriteLine(data);
            public void LogWarning(string data) => Console.WriteLine(data);
            public void LogError(string data) => Console.WriteLine(data);
            public void LogSummary(string data) => Console.WriteLine(data);
            public void LogInformationSummary(string data) => Console.WriteLine(data);
            public void LogErrorSummary(string data) => Console.WriteLine(data);
        }
    }
}