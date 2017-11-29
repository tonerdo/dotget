using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

using DotGet.Core.Configuration;
using DotGet.Core.Exceptions;
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

        public NuGetPackageResolver(string tool, ResolutionType resolutionType, ILogger logger)
            : base(tool, resolutionType, logger)
        {
            List<Lazy<INuGetResourceProvider>> providers = new List<Lazy<INuGetResourceProvider>>();
            providers.AddRange(Repository.Provider.GetCoreV3());

            _sourceRepository = new SourceRepository(new PackageSource(Globals.NuGetFeed), providers);
            _nuGetPackagesRoot = Path.Combine(Globals.GlobalNuGetDirectory, "packages");
            _nugetLogger = new NuGetLogger(Logger);
        }

        public override bool CanResolve()
            => !Tool.Contains("/") && !Tool.Contains(@"\") && !Tool.StartsWith(".");

        public override Options BuildOptions()
        {
            Options options = new Options();
            string[] parts = Tool.Split('@');

            options.Add("package", parts[0]);
            if (parts.Length > 1)
                options.Add("version", parts[1]);

            return options;
        }

        public override bool DidResolve(string command)
        {
            string path = GetPathFromCommand(command);
            return path.Contains(_nuGetPackagesRoot);
        }

        public override string GetToolName(string command)
        {
            string path = GetPathFromCommand(command);
            path = path.Replace(_nuGetPackagesRoot, string.Empty);
            path = path.Trim(Path.DirectorySeparatorChar).Trim(Path.AltDirectorySeparatorChar);
            var characters
                = path.TakeWhile((c) => c != Path.DirectorySeparatorChar || c != Path.AltDirectorySeparatorChar);
            return new String(characters.ToArray());
        }

        public override string Resolve()
        {
            bool hasVersion = Options.TryGetValue("version", out string version);
            string package = Options["package"];
            IPackageSearchMetadata packageSearchMetadata = GetPackageFromFeed(package, hasVersion && ResolutionType == ResolutionType.Install ? version : "");
            if (packageSearchMetadata == null)
            {
                string error = $"Could not find package {package}";
                if (hasVersion)
                    error += $" with version {version}";

                throw new ResolverException(error);
            }

            if (!HasNetCoreAppDependencyGroup(packageSearchMetadata))
                throw new ResolverException($"{package} does not support .NETCoreApp framework!");

            if (!RestoreNuGetPackage(packageSearchMetadata.Identity.Id, packageSearchMetadata.Identity.Version.ToFullString()))
                throw new ResolverException("Package install failed!");

            string netcoreappDirectory = packageSearchMetadata.DependencySets.Select(d => d.TargetFramework).LastOrDefault(t => t.Framework == ".NETCoreApp").GetShortFolderName();
            string dllDirectory = Path.Combine(_nuGetPackagesRoot, BuildPackageDirectoryPath(packageSearchMetadata.Identity.Id, packageSearchMetadata.Identity.Version.ToFullString()), "lib", netcoreappDirectory);

            DirectoryInfo directoryInfo = new DirectoryInfo(dllDirectory);
            FileInfo assembly = directoryInfo.GetFiles().FirstOrDefault(f => f.Extension == ".dll");
            if (assembly == null)
                throw new ResolverException("No assembly found in package!");

            return assembly.FullName;
        }

        private string GetPathFromCommand(string command)
        {
            string[] parts = command.Split(' ');
            string path = parts[1];
            if (parts.Length > 3)
                path = string.Join(string.Empty, parts.ToList().GetRange(1, parts.Length - 2));

            return path;
        }

        private bool HasNetCoreAppDependencyGroup(IPackageSearchMetadata package)
            => package.DependencySets.Any(d => d.TargetFramework.Framework == ".NETCoreApp");

        private IPackageSearchMetadata GetPackageFromFeed(string packageId, string version)
        {
            PackageMetadataResource packageMetadataResource = _sourceRepository.GetResource<PackageMetadataResource>();
            IEnumerable<IPackageSearchMetadata> searchMetadata = packageMetadataResource
                .GetMetadataAsync(packageId, true, true, _nugetLogger, CancellationToken.None).Result;

            return version == string.Empty
                ? searchMetadata.LastOrDefault() : searchMetadata.FirstOrDefault(s => s.Identity.Version.ToFullString() == version);
        }

        private bool RestoreNuGetPackage(string packageId, string version)
        {
            TargetFrameworkInformation tfi = new TargetFrameworkInformation() { FrameworkName = NuGetFramework.ParseFolder("netcoreapp2.0") };
            LibraryDependency dependency = new LibraryDependency
            {
                LibraryRange = new LibraryRange
                {
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
            RestoreCommandProviders restoreCommandProviders = RestoreCommandProviders.Create
            (
                _nuGetPackagesRoot,
                Enumerable.Empty<string>(),
                new SourceRepository[] { _sourceRepository },
                sourceCacheContext,
                new LocalNuspecCache(),
                _nugetLogger
            );

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