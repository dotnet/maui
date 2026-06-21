# .NET MAUI Release Process Using Arcade

This document describes the .NET MAUI release process, which uses the Arcade SDK for building packages and publishing them to NuGet.org and Workload Set channels.

## Overview

The .NET MAUI release process consists of two main phases:
1. Building and packing using `azure-pipelines-internal.yml`
2. Publishing to NuGet.org and Workload Set channels using `maui-release-internal.yml`

This process leverages the [.NET Arcade infrastructure](https://github.com/dotnet/arcade), which is a set of shared tools and services used across the .NET ecosystem to standardize build processes, dependency management, and package publishing.

## Build and Pack Pipeline (`azure-pipelines-internal.yml`)

The `azure-pipelines-internal.yml` pipeline is responsible for building, packing, and signing the .NET MAUI packages and workloads. This pipeline runs automatically on a schedule (daily at 5:00 UTC) for the main branch and is also triggered on commits to main, release branches, and tags. The pipeline runs in the internal Azure DevOps environment ([dnceng/internal](https://dev.azure.com/dnceng/internal/_git/dotnet-maui)) where it has access to signing certificates and secured resources.

### Key Steps in Build and Pack Pipeline

1. **Source Provisioning**: Sets up the build environment with necessary dependencies like Android SDKs.

2. **Build**: Builds the .NET MAUI projects in the repository.

3. **Pack**: Creates NuGet packages for the .NET MAUI libraries.

4. **Sign**: Signs the packages with Microsoft's certificate (only on official internal builds).

5. **Build Workloads**: Constructs the workload manifests and packages required for the .NET MAUI SDK.

6. **Publish to BAR**: After successful build and pack, the packages' metadata are published to the Build Asset Registry (BAR) in Maestro, which is used by the .NET SDK to consume these packages.

### Key Features

- The pipeline uses the Arcade SDK, which is a shared infrastructure for .NET projects
- All packages are signed using Microsoft's certificate
- Packages are published to internal feeds and registered in the BAR
- BAR IDs assigned to builds are used to track package assets throughout the release process
- Arcade provides dependency management, build orchestration, and signing services

## Release Pipeline (`maui-release-internal.yml`)

The `maui-release-internal.yml` pipeline is responsible for taking the packed artifacts and publishing them to the appropriate channels. This pipeline is not automatically triggered and must be manually run. Like the build pipeline, it also runs in the internal Azure DevOps environment where it has access to the necessary API keys and secured resources.

### Key Steps in Release Pipeline

1. **Publish to Workload Set Channel**: 
   - Takes the commit hash of the build to be released
   - Retrieves the Build Asset Registry (BAR) ID for that commit
   - Publishes the workload set to the appropriate .NET SDK workload channel (.NET 8, 9, or 10 Workload Release)
   - This allows the .NET SDK to consume the .NET MAUI workloads

2. **Release Packs**:
   - Requires manual approval
   - Takes the packages (excluding manifest packages) from the build
   - Pushes them to NuGet.org with retry logic and quota management

3. **Release Manifests**:
   - Requires separate manual approval
   - Takes only the manifest packages from the build
   - Pushes them to NuGet.org with retry logic and quota management

### Important Parameters

The release pipeline accepts several parameters:
- `commitHash`: The commit hash to download NuGet packages from
- `pushWorkloadSet`: Whether to publish to the Workload Set channel
- `pushNugetOrg`: Whether to push to NuGet.org
- `pushPackages`: Controls if packages are actually pushed (allows for dry runs)
- `nugetIncludeFilters` and `nugetExcludeFilters`: Filters for controlling which packages are published

## Release Flow

The complete release process follows these steps:

1. Build and package using `azure-pipelines-internal.yml`
   - This happens automatically on the main branch and release branches
   - The build produces NuGet packages and workload manifests
   - The build is assigned a BAR ID in Maestro
   - All assets are published to internal feeds and registered in the BAR using darc

2. Determine the commit hash of the build to be released

3. Run the `maui-release-internal.yml` pipeline with:
   - The commit hash of the build to release
   - Parameters to control which stages to run
   - Parameters to control which packages to include or exclude

4. The release pipeline uses Darc to:
   - Find the BAR ID associated with the specified commit
   - Gather all the packages and assets from internal channel
   - Publish the workload set to the appropriate .NET SDK workload channel

5. The release pipeline will then:
   - Publish the workload set to the appropriate .NET SDK workload channel
   - After manual approval, publish the NuGet packages to NuGet.org
   - After a separate manual approval, publish the manifest packages to NuGet.org

6. The packages are now available for consumption via:
   - NuGet.org for developers directly referencing the packages
   - .NET SDK's workload installation for the complete .NET MAUI development experience

## Build Environment

The .NET MAUI release process operates using two repositories:

1. **Public GitHub Repository**: [https://github.com/dotnet/maui](https://github.com/dotnet/maui)
   - Contains all source code and is the primary development repository
   - Used for public pull requests and non-official builds
   - All contributions and community engagement happen here

2. **Internal Azure DevOps Mirror**: [https://dev.azure.com/dnceng/internal/_git/dotnet-maui](https://dev.azure.com/dnceng/internal/_git/dotnet-maui)
   - An internal mirror of the GitHub repository
   - Used for official builds, signing, and release processes
   - Contains the same code but runs in a secured environment with access to signing certificates and internal resources
   - The `azure-pipelines-internal.yml` and `maui-release-internal.yml` pipelines run against this mirror

The use of the internal mirror ensures that the signing process and access to internal feeds are properly secured while still maintaining an open-source development model in the public repository. Changes are synchronized from the public repository to the internal mirror, ensuring that the released packages contain the same code that is publicly visible.

## Arcade, Darc, and Maestro

### Arcade Infrastructure

[Arcade](https://github.com/dotnet/arcade) is Microsoft's .NET Core engineering system, providing shared tools, SDK, and services for .NET repository builds. Arcade standardizes:

- Build environments and scripts
- Package versioning
- Dependency management
- Signing and publishing
- CI/CD integration

### Darc: Dependency and Asset Reuse Coordinator

Darc is a tool within the Arcade infrastructure used to manage dependencies and coordinate asset reuse across the .NET ecosystem. In the .NET MAUI release process, Darc plays a critical role:

1. **Dependency Management**: Manages dependencies between .NET MAUI and other .NET repositories.
2. **Build Asset Tracking**: Associates builds with channels and creates records in the Build Asset Registry (BAR).
3. **Package Discovery**: Locates packages produced by specific commits.
4. **Asset Gathering**: Collects packages for publishing from internal feeds.

### Maestro and the Build Asset Registry (BAR)

Maestro is the service that maintains the Build Asset Registry (BAR), which serves as the central database tracking all assets (packages, blobs, etc.) produced by .NET builds. The release process uses Maestro to:

1. **Track Builds**: Each build is assigned a unique BAR ID that catalogs all assets produced by that build.
2. **Channel Association**: Builds are associated with channels like ".NET 8 Workload Release" or ".NET 9 Workload Release".
3. **Asset Discovery**: The release pipeline uses the BAR ID to locate and gather all assets needed for publishing.

### Publishing Process Flow with Darc

The `maui-release-internal.yml` pipeline uses Darc to:

1. **Find the Build**: Using the commit hash parameter, Darc identifies the corresponding build in the BAR.
   ```powershell
   $buildJson = & $darc get-build --ci --repo "${{ parameters.ghRepo }}" --commit "$(COMMIT)" --output-format json
   $barId = $buildJson | ConvertFrom-Json | Select-Object -ExpandProperty "id" -First 1
   ```

2. **Gather Assets**: Once the BAR ID is obtained, Darc collects all the packages from internal feeds.
   ```powershell
   & $darc gather-drop --ci --id $barId -o "$(Build.StagingDirectory)\nupkgs" --azdev-pat $(System.AccessToken) --verbose
   ```

3. **Channel Association**: For workload sets, Darc adds the build to the appropriate workload channel.
   ```powershell
   & $darc add-build-to-channel --ci --channel "$workloadSetsChannel" --id "$barId" --skip-assets-publishing
   ```

4. **Publishing**: The gathered packages are then published to NuGet.org and other appropriate channels based on pipeline parameters.

## Notes

- The release process requires appropriate permissions and API keys for the NuGet feeds, these are stored normally on a KeyVault that is associated with the pipeline
- Multiple API keys are used to handle NuGet.org's rate limiting and quota restrictions
- The process includes retry logic to handle transient failures
- Both pipelines run on internal Microsoft infrastructure to ensure security
- All official builds and releases are performed from the internal Azure DevOps mirror repository, not directly from GitHub
- The internal repository is synchronized with the public GitHub repository to ensure released code matches public code

## References

- [Arcade SDK Documentation](https://github.com/dotnet/arcade/blob/main/Documentation/README.md)
- [.NET MAUI Workloads Documentation](https://github.com/dotnet/maui/blob/main/src/Workload/README.md)
- [Darc Documentation](https://github.com/dotnet/arcade/blob/main/Documentation/Darc.md)
- [Maestro and BAR Overview](https://github.com/dotnet/arcade/blob/main/Documentation/Maestro.md)
