using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

using DotGet.Core.Configuration;

using NuGet.Commands;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.LibraryModel;
using NuGet.ProjectModel;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace DotGet.Core.Resolvers
{
    internal class NuGetPackageResolver : Resolver
    {
        private SourceRepository _sourceRepository;
        private string _nuGetPackagesRoot;

        public NuGetPackageResolver(string tool, ResolverOptions options) : base(tool, options)
        {
            bool customNuGetFeed = options.TryGetValue("feed", out string nuGetFeed);
            nuGetFeed = customNuGetFeed ? nuGetFeed : "https://api.nuget.org/v3/index.json";

            List<Lazy<INuGetResourceProvider>> providers = new List<Lazy<INuGetResourceProvider>>();
            providers.AddRange(Repository.Provider.GetCoreV3());
            _sourceRepository = new SourceRepository(new PackageSource(nuGetFeed), providers);

            _nuGetPackagesRoot = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Environment.GetEnvironmentVariable("USERPROFILE") : Environment.GetEnvironmentVariable("HOME");
            _nuGetPackagesRoot = Path.Combine(_nuGetPackagesRoot, ".nuget", "packages");
        }

        public override bool CanResolve() => !Tool.Contains("/") && !Tool.Contains(@"\") && !Tool.StartsWith(".");

        public override (bool, string) Resolve()
        {
            IPackageSearchMetadata package = GetPackageFromFeed(Tool);
            if (package == null)
                return (false, "Package not found!");

            bool isNetCoreApp = HasNetCoreAppDependencyGroup(package);
            if (!isNetCoreApp)
                return (false, "Package does not support .NETCoreApp!");

            bool success = InstallNuGetPackage(package.Identity.Id, package.Identity.Version.ToFullString());
            if (!success)
                return (false, "Package installed failed!");

            string netcoreappDirectory = package.DependencySets.Select(d => d.TargetFramework).LastOrDefault(t => t.Framework == ".NETCoreApp").GetShortFolderName();
            string dllDirectory = Path.Combine(_nuGetPackagesRoot, BuildPackageDirectoryPath(package.Identity.Id, package.Identity.Version.ToFullString()), "lib", netcoreappDirectory);

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
            PackageMetadataResource packageMetadataResource = _sourceRepository.GetResource<PackageMetadataResource>();
            IEnumerable<IPackageSearchMetadata> searchMetadata = packageMetadataResource
                .GetMetadataAsync(packageId, true, true, new Logger(), CancellationToken.None).Result;

            return version == string.Empty
                ? searchMetadata.LastOrDefault() : searchMetadata.FirstOrDefault(s => s.Identity.Version.ToFullString() == version);
        }

        private bool InstallNuGetPackage(string packageId, string version)
        {
            TargetFrameworkInformation tfi = new TargetFrameworkInformation() { FrameworkName = NuGetFramework.ParseFolder("netcoreapp1.1") };
            LibraryDependency dependency = new LibraryDependency {
                LibraryRange = new LibraryRange {
                    Name = packageId,
                    VersionRange = VersionRange.Parse(version),
                    TypeConstraint = LibraryDependencyTarget.Package
                }
            };

            PackageSpec spec = new PackageSpec(new List<TargetFrameworkInformation>() { tfi });
            spec.Name = "TempProj";
            spec.Dependencies = new List<LibraryDependency>() { dependency };
            spec.RestoreMetadata = new ProjectRestoreMetadata() { ProjectPath = "TempProj.csproj" };

            SourceCacheContext sourceCacheContext = new SourceCacheContext { DirectDownload = true, IgnoreFailedSources = false };
            RestoreCommandProviders restoreCommandProviders = RestoreCommandProviders.Create(_nuGetPackagesRoot, Enumerable.Empty<string>(), new SourceRepository[] { _sourceRepository }, sourceCacheContext, new Logger());
            RestoreRequest restoreRequest = new RestoreRequest(spec, restoreCommandProviders, sourceCacheContext, new Logger());
            restoreRequest.LockFilePath = Path.Combine(AppContext.BaseDirectory, "project.assets.json");
            restoreRequest.ProjectStyle = ProjectStyle.PackageReference;
            restoreRequest.RestoreOutputPath = _nuGetPackagesRoot;

            RestoreCommand restoreCommand = new RestoreCommand(restoreRequest);
            return restoreCommand.ExecuteAsync().Result.Success;
        }

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