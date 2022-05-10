# Building `maui.aar`

See [HOWTOBUILD](HOWTOBUILD) for
details on building with `gradle`.

However, since this builds an `.aar` (instead of a `.jar`) the
top-level task is:

    .\gradlew createAar --rerun-tasks

## Glide
NOTE: The binding nuget package version for glide specified in `eng/Version.props`
must be kept in sync with the maven artifact specified in this project
in the `maui/build.gradle`!