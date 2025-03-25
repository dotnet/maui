The `eng` folder contains and is used by parts of the https://github.com/dotnet/arcade SDK.

## Dependency Management
The Arcade SDK contains a tool known as [`darc`][0], which can be used to manage
and query the relationships between repositories in the dotnet ecosystem.

The `eng/Version.Details.xml` and `eng/Versions.props` files contain information
about the products and tooling that this repository depends on.

Many dotnet repositories use a publishing workflow that will push build artifact data
to a central location known as the "Build Asset Registry".  This data includes
a "channel" association, which is used to determine when an update for a particular
product or tool is available.  Local updates and automatic update "subscriptions"
compare the version files in the repository against the versions available in the
channel that you are interested in.  The `darc` tool is used facilitate these updates.

To work with `darc` locally, see the [setting up your darc client docs][1].
You'll need to run a script in the dotnet/arcade repo to install the dotnet global
tool, join the `arcade-contrib` GitHub team, and run the [`darc authenticate`][2]
command to add the PATs required by the tool.

The GitHub PAT that you add must have the full `repo` scope enabled if you want to
work with any of the `subcription` commands.  Subscriptions control the automated
creation of dependency update pull requests.


To add a new dependency, run the [`darc add-dependency`][3] command at the root
of the repository:
```
darc add-dependency -n Microsoft.Dotnet.Sdk.Internal -t product -v 6.0.0-preview.2.21154.6 -r https://github.com/dotnet/installer
```

To update all dependencies, use the [`darc update-dependencies`][4] command:
```
darc update-dependencies --channel ".NET 6"
```

To configure automatic updates, use the [`darc add-subscription`][5] command
to enroll a target repo/branch into updates from a particular channel:
```
darc add-subscription --channel ".NET 6" --source-repo https://github.com/dotnet/installer --target-repo https://github.com/dotnet/maui --target-branch main --update-frequency everyWeek --standard-automerge
```

Once a subscription is configured, pull requests will be created automatically
by the dotnet Maestro bot whenever dependency updates are available.

Subscriptions need to be manually managed at this time.  For example, when a
release branch is created, someone with `darc` installed locally will need to
run the `add-subscription` command to configure updates against that new branch.


#### Build Asset Manifest Promotion

Builds from main and release branches will push NuGet package metadata to the
darc/maestro Build Asset Registry.  This build information will also be promoted
to a default darc/maestro channel if one is configured.  Default channels are
manually managed at this time.  To configure a new default repo+branch <-> channel
association, run the [`darc add-default-channel`][6] command:
```
darc add-default-channel --channel ".NET 9.0.1xx SDK" --branch "net9.0" --repo https://github.com/dotnet/maui
```

When a new release branch is created, this command should look something like this:
```
darc add-default-channel --channel ".NET 9.0.1xx SDK Preview 1" --branch "release/9.0.1xx-preview1" --repo https://github.com/dotnet/maui
```

Other products/tools can consume our package version info in the following way:
```
darc add-dependency -n Microsoft.Maui.Sdk -t product -r https://github.com/dotnet/maui -v 1.2.3
```


[0]: https://github.com/dotnet/arcade/blob/ea609b8e036359934332480de9336d98fcbb3f91/Documentation/Darc.md
[1]: https://github.com/dotnet/arcade/blob/ea609b8e036359934332480de9336d98fcbb3f91/Documentation/Darc.md#setting-up-your-darc-client
[2]: https://github.com/dotnet/arcade/blob/ea609b8e036359934332480de9336d98fcbb3f91/Documentation/Darc.md#authenticate
[3]: https://github.com/dotnet/arcade/blob/ea609b8e036359934332480de9336d98fcbb3f91/Documentation/Darc.md#add-dependency
[4]: https://github.com/dotnet/arcade/blob/ea609b8e036359934332480de9336d98fcbb3f91/Documentation/Darc.md#update-dependencies
[5]: https://github.com/dotnet/arcade/blob/ea609b8e036359934332480de9336d98fcbb3f91/Documentation/Darc.md#add-subscription
[6]: https://github.com/dotnet/arcade/blob/ea609b8e036359934332480de9336d98fcbb3f91/Documentation/Darc.md#add-default-channel
