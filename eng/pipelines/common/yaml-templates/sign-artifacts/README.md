**IMPORTANT NOTE:** Unlike our happy cousin on Jenkins, we need to get each pipeline approved by the big guns: https://devdiv.visualstudio.com/DevDiv/_wiki/wikis/DevDiv.wiki/713/Signing-Authorized There are works like "reported" and "mails going out" and "email alerts"... Ooooh, scary. Basically, approve first, but the TEST signing will work without approval. 

## Use Case

It seems Jenkins is no longer the cool boy at the party...

Although using the new MicroBuild is pretty easy to use if you have a simple case, a cross-platform solution gets a bit tricky. In most cases, all that needs to be done is:
 - reference the special NuGet
 - run a special setup pipeline task
 - use MSBuild to build or pack as normal which will sign everything inline
 - run a special cleanup task

However! This is not always so great:
 - the assemblies are built on macOS or Linux
 - the packages are built up from various files from different bots

So, this PR works to provide a migration for existing builds as well as to still support the edge cases that the MicroBuild NuGets don't yet support.

## Implementation

The current implementation is a bit limited to what I need right now: sign a folder filled with .nupkg and zip files. This PR consists of 2 templates: steps and job.

The STEPS template is really the bit that goes and:
 - collects all the .nupkg and zip files
 - extracts all the .nupkg and zip files
 - signs all the .dlls and .exes
 - repacks the .nupkg files or zip files
 - signs the .nupkg files or zip files

The JOB template tries to match existing functionality by:
 - downloading the current 'nuget' or 'zip' artifact for the pipeline
 - run the signing steps
 - uploads the signed artifacts as 'nuget-signed' or 'zip-signed'

For both cases, the `SignType` is "detected" but is also overridable with the `signType` parameter. Also, the `teamName` defaults to "Xamarin" because we are a cool bunch. But, this may need changing - for example the XF team might want "Maui"

Under the hood, I just use a fake MSBuild project and use the same build tasks that a normal project would - but without the auto-hooks into the build and pack.

## Use

Following the patter of old where all that was required was to publish a artifact called "nuget" or "zip", you just need to add this:

```yaml
jobs:
  - ${{ if eq(variables['System.TeamProject'], 'devdiv') }}:
    - template: sign-artifacts/jobs/v2.yml@internal-templates
      parameters:
        targetFolder: 'output/signed'
```

If you want to use the steps, then you can use:

```yaml
steps:
  - ${{ if eq(variables['System.TeamProject'], 'devdiv') }}:
    - template: sign-artifacts/steps/v2.yml@internal-templates
      parameters:
        sourceFolder: 'output/nugets'
        targetFolder: 'output/signed'
```

## Warning
This will unzip all files to the same place and then return a single zip file currently.

## Shared Pipeline

**DO NOT USE**

> this may be a security risk, however can be used as a reference.

~~In some cases, it may be useful to not actually do the signing in the actual pipeline, but rather request that another pipeline download and sign the artifacts. This prevents the actual pipline for ever downloading any actual information as it just requests a build for the signing pipeline. The signing pipeline actually downloads the secrets, templates and uses an internal agent. Once the signing is complete, the original pipeline can then just extract the signed artifacts.~~

~~Using this method for signing is just a matter of importing the request template after uploading the `nuget` artifact to be signed:~~

```yaml
steps:
  # upload artifacts with a known name (eg: nuget)
  - ${{ if eq(variables['System.TeamProject'], 'devdiv') }}:
    - template: sign-artifacts/steps/v2-request.yml@internal-templates
      parameters:
        targetFolder: 'output/signed'
```

## Controlling the Signing

To avoid the possibility of a file being signed incorrectly, make the actual file list something that needs to be provided. For example:

```xml
<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">

  <ItemGroup>
    <FirstParty Include="Xamarin.AndroidX.*.dll" />
  </ItemGroup>

  <ItemGroup>
    <ThirdParty Include="Newtonsoft.Json.dll" />
  </ItemGroup>

  <ItemGroup>
    <Skip Include="System.*.dll" />
  </ItemGroup>

</Project>
```

For this to work, you can either set the `$(SignListPath)` property to the exact path of the file, or just place a file named `SignList.xml` in the root of the source directory.

The item group names are pretty simple but should not have conflicts as this project is totally custom and only consists of the .proj file and the nuget imports.

Additionally, a `SignList.targets` file can be used to inject custom targets into the MSBuild based signing process. Authors who opt-in to this file must import it themselves somewhere in `SignList.xml`. See https://github.com/xamarin/xamarin-macios/pull/12400 for an example.
