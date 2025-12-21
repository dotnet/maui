---
description: "Guidance for GitHub Copilot when working with .NET Arcade pipeline files"
applyTo: "eng/pipelines/arcade/**"
---

# .NET Arcade Pipeline Guidelines

This document provides guidance for working with Azure DevOps pipeline files in the `eng/pipelines/arcade/` folder. These files are part of the .NET Arcade infrastructure, which provides shared build and CI/CD tooling across .NET Foundation projects.

## ⚠️ Important: Consult Arcade Documentation First

**Before making any changes to files in this folder, always consult the official Arcade documentation:**

- **Main Repository**: https://github.com/dotnet/arcade
- **Arcade SDK Documentation**: https://github.com/dotnet/arcade/blob/main/Documentation/ArcadeSdk.md
- **Azure DevOps Onboarding**: https://github.com/dotnet/arcade/blob/main/Documentation/AzureDevOps/AzureDevOpsOnboarding.md

## Folder Structure

```
eng/pipelines/arcade/
├── setup-test-env.yml           # Test environment setup template
├── stage-api-scan.yml           # API scanning stage
├── stage-build.yml              # Build stage template
├── stage-device-tests.yml       # Device tests stage
├── stage-helix-tests.yml        # Helix distributed tests stage
├── stage-integration-tests.yml  # Integration tests stage
├── stage-pack.yml               # Packaging stage
├── stage-unit-tests.yml         # Unit tests stage
└── variables.yml                # Shared pipeline variables
```

## Key Conventions

### 1. Stage Templates

Files prefixed with `stage-` are reusable stage templates. They define:
- `parameters:` - Input parameters with defaults
- `stages:` - One or more pipeline stages with jobs and steps

**Example structure:**
```yaml
parameters:
  stageDependsOn: []
  jobMatrix: []
  mauiSourcePath: $(Build.SourcesDirectory)
  buildConfig: Debug

stages:
- stage: StageName
  displayName: Stage Display Name
  dependsOn: ${{ parameters.stageDependsOn }}
  jobs:
  - ${{ each job in parameters.jobMatrix }}:
    - job: ${{ job.name }}
      # ... job configuration
```

### 2. Template References

Always use absolute paths from repository root when referencing templates:
```yaml
# ✅ Correct - absolute path from root
- template: /eng/pipelines/arcade/setup-test-env.yml
- template: /eng/pipelines/common/run-dotnet-preview.yml

# ❌ Incorrect - relative paths
- template: ../common/run-dotnet-preview.yml
```

### 3. Conditions and Error Handling

#### Always Publish Test Results
Use `condition: always()` on `PublishTestResults` tasks to ensure results are published even when tests fail:
```yaml
- task: PublishTestResults@2
  inputs:
    testResultsFormat: VSTest
    testResultsFiles: $(Agent.TempDirectory)/**/*.trx
    testRunTitle: Test Run Name
  condition: always()  # ← Important!
```

#### Fail-On-Issue Template
Include the `fail-on-issue.yml` template at the end of job steps to ensure jobs fail when issues are logged:
```yaml
- template: /eng/pipelines/common/fail-on-issue.yml
```

**Do NOT comment out this template** - it ensures test failures are properly reported to Azure DevOps.

### 4. Variables

Use Arcade-provided variables where possible:
- `$(Build.SourcesDirectory)` - Repository root
- `$(Build.Arcade.LogsPath)` - Standard log output path
- `$(Agent.TempDirectory)` - Temporary working directory
- `$(System.JobAttempt)` - Current job retry attempt number

### 5. 1ES Pipeline Templates

For publishing artifacts, use the `publishTaskPrefix` parameter:
```yaml
- task: ${{ parameters.publishTaskPrefix }}PublishPipelineArtifact@1
  displayName: Publish Logs
  inputs:
    targetPath: ${{ parameters.repoLogPath }}
    artifact: Artifact Name $(System.JobAttempt)
  condition: always()
```

## Common Patterns

### Job Matrix Pattern

Stage templates use `jobMatrix` to define multiple jobs with different configurations:
```yaml
parameters:
  jobMatrix: []  # Expects: name, displayName, pool, timeout, testCategory

jobs:
- ${{ each job in parameters.jobMatrix }}:
  - job: ${{ job.name }}
    displayName: ${{ job.displayName }}
    pool: ${{ job.pool }}
    timeoutInMinutes: ${{ job.timeout }}
```

### Conditional Steps

Use YAML conditionals for platform-specific or category-specific logic:
```yaml
${{ if eq(job.testCategory, 'RunOniOS') }}:
  envVariables:
    IOS_TEST_DEVICE: ios-simulator-64_18.5
```

## Do's and Don'ts

### ✅ Do

- Consult Arcade documentation before making changes
- Use `condition: always()` for test result publishing
- Include `fail-on-issue.yml` to ensure failures are reported
- Use absolute template paths from repository root
- Follow existing patterns in the codebase
- Test pipeline changes by running a build

### ❌ Don't

- Comment out `fail-on-issue.yml` (this hides test failures)
- Use relative template paths
- Modify Arcade SDK files directly (they're managed by Arcade)
- Ignore Arcade variable conventions
- Skip test result publishing conditions

## Testing Pipeline Changes

Pipeline changes should be tested by:
1. Pushing the branch and triggering a PR build
2. Reviewing the Azure DevOps build logs
3. Verifying test results appear in the Tests tab
4. Checking that failures are properly reported

## Related Files

- `/eng/pipelines/common/` - Common pipeline templates shared across stages
- `/eng/common/` - Arcade SDK managed files (do not modify)
- `/eng/Versions.props` - Version management
- `/global.json` - SDK version configuration

## External References

- [.NET Arcade Repository](https://github.com/dotnet/arcade)
- [Arcade SDK Documentation](https://github.com/dotnet/arcade/blob/main/Documentation/ArcadeSdk.md)
- [Azure DevOps YAML Schema](https://learn.microsoft.com/en-us/azure/devops/pipelines/yaml-schema/)
- [Azure Pipelines Conditions](https://learn.microsoft.com/en-us/azure/devops/pipelines/process/conditions)
