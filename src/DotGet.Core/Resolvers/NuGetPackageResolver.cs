using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

using DotGet.Core.Configuration;
using DotGet.Core.Logging;

using NuGet.Commands;
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
        private NuGetLogger _nugetLogger;

        public NuGetPackageResolver(string tool, ResolverOptions options, ILogger logger) : base(tool, options, logger)
        {
            bool customNuGetFeed = options.TryGetValue("feed", out string nuGetFeed);
            nuGetFeed = customNuGetFeed ? nuGetFeed : "https://api.nuget.org/v3/index.json";

            List<Lazy<INuGetResourceProvider>> providers = new List<Lazy<INuGetResourceProvider>>();
            providers.AddRange(Repository.Provider.GetCoreV3());
            _sourceRepository = new SourceRepository(new PackageSource(nuGetFeed), providers);

            _nuGetPackagesRoot = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Environment.GetEnvironmentVariable("USERPROFILE") : Environment.GetEnvironmentVariable("HOME");
            _nuGetPackagesRoot = Path.Combine(_nuGetPackagesRoot, ".nuget", "packages");

            _nugetLogger = new NuGetLogger(Logger);
        }

        public override bool CanResolve() => !Tool.Contains("/") && !Tool.Contains(@"\") && !Tool.StartsWith(".");

        public override (bool, string) Resolve()
        {
            IPackageSearchMetadata package = GetPackageFromFeed(Tool);
            if (package == null)
            {
                Logger.LogError($"Could not find package {Tool}");
                return (false, null);
            }

            bool isNetCoreApp = HasNetCoreAppDependencyGroup(package);
            if (!isNetCoreApp)
            {
                Logger.LogError($"{Tool} does not support framework .NETCoreApp!");
                return (false, null);
            }

            bool success = InstallNuGetPackage(package.Identity.Id, package.Identity.Version.ToFullString());
            if (!success)
            {
                Logger.LogError("Package installed failed!");
                return (false, null);
            }

            string netcoreappDirectory = package.DependencySets.Select(d => d.TargetFramework).LastOrDefault(t => t.Framework == ".NETCoreApp").GetShortFolderName();
            string dllDirectory = Path.Combine(_nuGetPackagesRoot, BuildPackageDirectoryPath(package.Identity.Id, package.Identity.Version.ToFullString()), "lib", netcoreappDirectory);

            DirectoryInfo directoryInfo = new DirectoryInfo(dllDirectory);
            FileInfo dll = directoryInfo.GetFiles().FirstOrDefault(f => f.Extension == ".dll");
            if (dll == null)
            {
                Logger.LogError("No executable found in package!");
                return (false, null);
            }

            return (true, dll.FullName);
        }

        private bool HasNetCoreAppDependencyGroup(IPackageSearchMetadata package)
            => package.DependencySets.Where(d => d.TargetFramework.Framework == ".NETCoreApp").Count() > 0;

        private IPackageSearchMetadata GetPackageFromFeed(string packageId, string version = "")
        {
            PackageMetadataResource packageMetadataResource = _sourceRepository.GetResource<PackageMetadataResource>();
            IEnumerable<IPackageSearchMetadata> searchMetadata = packageMetadataResource
                .GetMetadataAsync(packageId, true, true, _nugetLogger, CancellationToken.None).Result;

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
            RestoreCommandProviders restoreCommandProviders = RestoreCommandProviders.Create(_nuGetPackagesRoot, Enumerable.Empty<string>(), new SourceRepository[] { _sourceRepository }, sourceCacheContext, _nugetLogger);
            RestoreRequest restoreRequest = new RestoreRequest(spec, restoreCommandProviders, sourceCacheContext, _nugetLogger);
            restoreRequest.LockFilePath = Path.Combine(AppContext.BaseDirectory, "project.assets.json");
            restoreRequest.ProjectStyle = ProjectStyle.PackageReference;
            restoreRequest.RestoreOutputPath = _nuGetPackagesRoot;

            RestoreCommand restoreCommand = new RestoreCommand(restoreRequest);
            return restoreCommand.ExecuteAsync().Result.Success;
        }

        private string BuildPackageDirectoryPath(string packageId, string version) => Path.Combine(packageId.ToLower(), version);
    }
}