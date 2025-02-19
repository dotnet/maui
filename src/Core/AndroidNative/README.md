# Building `maui.aar`

The `@(AndroidGradleProject)` element declared in `src/Core/src/Core.csproj`
will create and pack `maui.aar` when building that project.

Alternatively, see [HOWTOBUILD](HOWTOBUILD) for
details on building with `gradle`.

## Glide
NOTE: The binding nuget package version for glide specified in `eng/Version.props`
must be kept in sync with the maven artifact specified in this project
in the `maui/build.gradle`!