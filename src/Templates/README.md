
# MAUI Templates

For easy building and testing you can use the `build.ps1` script. This script is only for manual use and not part of any pipeline.

> [!NOTE]
> On macOS you find encounter and error like: `error NU5119: Warning As Error: File '/file/path/.DS_Store' was not added to the package. Files and folders starting with '.' or ending with '.nupkg' are excluded by default. To include this file, use -NoDefaultExcludes from the commandline` when this happens, run a `git clean -xfd` on the repository to remove all `.DS_Store` files from the filesystem.

## Functionality

The script:
* Deletes the `.tempTemplateOutput` folder which is used for the temporary files used by this script
* Builds the `src\Templates\src\Microsoft.Maui.Templates.csproj` project
* Packs the `src\Templates\src\Microsoft.Maui.Templates.csproj` project into a .nupkg file and outputs it to the `.tempTemplateOutput` directory, this directly is excluded from git
* Uninstalls any previous manual installations of .NET MAUI templates
* Empties the `~\templateengine` folder
* Finds and installs the resulting .nupkg artifact in the `.tempTemplateOutput` directory
* Creates a new .NET MAUI project based on the latest changes in the template
* Opens the new .NET MAUI project in Visual Studio (or on Mac in Visual Studio Code)

## Parameters

The script defines a coupe of parameters you can use. All have default values, so you only have to set them whenever you want to deviate from the default behavior.

The parameters are as follows:

* `projectType`: Specifies the type of .NET project to create (default is `maui`).
* `templateVersion`: Specifies the version number to use for the template pack build (default is `13.3.7`, needs to be a valid major, minor, patch version number, for example 1.2.3).
* `templatesProjectPath`: Specifies the path to the template project to build (default is `src\Microsoft.Maui.Templates.csproj`).
* `startVsAfterBuild`: Specifies whether to start Visual Studio (Code) after creating the new project with the latest template changes (default is `true`).

### Example usage with parameters

Find sample usages of the different parameters below, of course these can be mixed and matched as needed.

* Instead of a default .NET MAUI app, use the Blazor Hybrid template: `.\build.ps1 -projectType maui-blazor`
* Set a custom version number for the template: `.\build.ps1 -templateVersion 1.2.3`
* Build another template project: `.\build.ps1 -templatesProjectPath src\Microsoft.Maui.Templates-new.csproj`
* Don't start VS after creating the new project using the latest template changes: `.\build.ps1 -startVsAfterBuild $false`