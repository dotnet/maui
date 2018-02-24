#tool "mdoc"

///////////////////////////////////////////////////////////////////////////////
// TOOLS
///////////////////////////////////////////////////////////////////////////////

var nugetPath = Context.Tools.Resolve("nuget.exe");
var mdocPath = Context.Tools.Resolve("mdoc.exe");

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("UpdateDocs")
    .Does(() =>
{
    // restore nugets
    NuGetRestore("./Caboodle.sln", new NuGetRestoreSettings { 
        ToolPath = nugetPath,
    });

    // build the project
    MSBuild("./Caboodle.sln", new MSBuildSettings {
        Configuration = configuration,
        PlatformTarget = PlatformTarget.MSIL,
        MSBuildPlatform = MSBuildPlatform.x86,
    });

    // the reference folders to locate assemblies
    var refDirs = new List<DirectoryPath>();

    // only Windows needs this, macOS finds everything automatically
    if (IsRunningOnWindows ()) {
        var refNetNative = "C:/Program Files (x86)/MSBuild/15.0/.Net/.NetNative/*/x86/ilc/lib/Private";
        var refAssemblies = "C:/Program Files (x86)/Microsoft Visual Studio/*/*/Common7/IDE/ReferenceAssemblies/Microsoft/Framework/";
        refDirs.AddRange(GetDirectories(refNetNative));
        refDirs.AddRange(GetDirectories(refAssemblies + "MonoAndroid/v1.0"));
        refDirs.AddRange(GetDirectories(refAssemblies + "MonoAndroid/v4.0.3"));
        refDirs.AddRange(GetDirectories(refAssemblies + "Xamarin.iOS/v1.0"));
        refDirs.AddRange(GetDirectories(refAssemblies + "Xamarin.TVOS/v1.0"));
        refDirs.AddRange(GetDirectories(refAssemblies + "Xamarin.WatchOS/v1.0"));
        refDirs.AddRange(GetDirectories(refAssemblies + "Xamarin.Mac/v2.0"));
    }

    // the assemblies to generate documentation for
    var assemblies = GetFiles($"./Caboodle/bin/{configuration}/**/*.dll");

    // generate the docs
    var args = new ProcessArgumentBuilder()
        .Append("update")
        .Append("--preserve")
        .AppendSwitchQuoted("--out", "=", "./docs/en/");
    foreach (var refDir in refDirs) {
        args = args.AppendSwitchQuoted("--lib", "=", refDir.FullPath);
    }
    foreach (var assembly in assemblies) {
        args = args.AppendQuoted(assembly.FullPath);
    }
    StartProcess(mdocPath, new ProcessSettings {
        Arguments = args
    });
});

RunTarget(target);
