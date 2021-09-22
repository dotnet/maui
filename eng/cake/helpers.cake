Task("Clean")
    .Description("Deletes all the obj/bin directories")
    .Does(() =>
{
    List<string> foldersToClean = new List<string>();

    foreach (var item in new [] {"obj", "bin"})
    {
        foreach(string f in System.IO.Directory.GetDirectories(".", item, SearchOption.AllDirectories))
        {
            if(f.StartsWith(@".\bin") || f.StartsWith(@".\tools"))
                continue;

            // this is here as a safety check
            if(!f.StartsWith(@".\src"))
                continue;

            CleanDirectories(f);
        }        
    } 
});



T GetBuildVariable<T>(string key, T defaultValue)
{
    // on MAC all environment variables are upper case regardless of how you specify them in devops
    // And then Environment Variable check is case sensitive
    T upperCaseReturnValue = Argument(key.ToUpper(), EnvironmentVariable(key.ToUpper(), defaultValue));
    return Argument(key, EnvironmentVariable(key, upperCaseReturnValue));
}

string GetAndroidSDKPath()
{
    var ANDROID_SDK_ROOT = Argument("android", EnvironmentVariable("ANDROID_SDK_ROOT") ?? EnvironmentVariable("ANDROID_HOME"));

    if (string.IsNullOrEmpty(ANDROID_SDK_ROOT)) {
        throw new Exception("Environment variable 'ANDROID_SDK_ROOT' or 'ANDROID_HOME' must be set to the Android SDK root.");    
    }

    return ANDROID_SDK_ROOT;
}

public void PrintEnvironmentVariables()
{
    var envVars = EnvironmentVariables();

    string path;
    if (envVars.TryGetValue("PATH", out path))
    {
        Information("Path: {0}", path);
    }

    foreach(var envVar in envVars)
    {
        Information(
            "Key: {0}\tValue: \"{1}\"",
            envVar.Key,
            envVar.Value
            );
    };
}

void SetDotNetEnvironmentVariables(string dotnetDir)
{
    var dotnet = dotnetDir ?? MakeAbsolute(Directory("./bin/dotnet/")).ToString();

    SetEnvironmentVariable("DOTNET_INSTALL_DIR", dotnet);
    SetEnvironmentVariable("DOTNET_ROOT", dotnet);
    SetEnvironmentVariable("DOTNET_MSBUILD_SDK_RESOLVER_CLI_DIR", dotnet);
    SetEnvironmentVariable("DOTNET_MULTILEVEL_LOOKUP", "0");
    SetEnvironmentVariable("MSBuildEnableWorkloadResolver", "true");
    SetEnvironmentVariable("PATH", dotnet, prepend: true);
}

void SetEnvironmentVariable(string name, string value, bool prepend = false)
{
    var target = EnvironmentVariableTarget.Process;

    if (prepend)
        value = value + System.IO.Path.PathSeparator + EnvironmentVariable(name);

    Environment.SetEnvironmentVariable(name, value, target);

    Information("Setting environment variable: {0} = '{1}'", name, value);
}
