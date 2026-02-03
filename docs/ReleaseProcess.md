# .NET MAUI Release Process Using Arcade

This document describes the .NET MAUI release process, which uses the Arcade SDK for building packages and publishing them to NuGet.org and Workload Set channels.

## Overview

The .NET MAUI release process uses `azure-pipelines-internal.yml` for building, packing, and publishing packages.

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

## Release and Publishing

The `azure-pipelines-internal.yml` pipeline handles both building and publishing to the appropriate channels. Publishing happens automatically after successful build and signing, with packages being published to internal feeds and registered in the Build Asset Registry (BAR) using Darc.

## Release Flow

The complete release process follows these steps:

1. Build and package using `azure-pipelines-internal.yml`
   - This happens automatically on the main branch and release branches
   - The build produces NuGet packages and workload manifests
   - The build is assigned a BAR ID in Maestro
   - All assets are published to internal feeds and registered in the BAR using Darc

2. Packages are published to appropriate channels:
   - Published to internal feeds for consumption by other .NET repositories
   - The workload set is published to the appropriate .NET SDK workload channel
   - After required approvals, packages are published to NuGet.org

3. The packages are now available for consumption via:
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
   - The `azure-pipelines-internal.yml` pipeline runs against this mirror

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

Darc is used during the build process to:

1. **Track the Build**: Each build is assigned a unique BAR ID that catalogs all assets produced.

2. **Publish Assets**: All packages are published to internal feeds and registered in the BAR.

3. **Channel Association**: Builds are added to the appropriate workload channel (e.g., ".NET 8 Workload Release", ".NET 9 Workload Release").

4. **Downstream Distribution**: The gathered packages are then published to NuGet.org and other appropriate channels as part of the build pipeline.

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
