#addin nuget:?package=Cake.AppleSimulator&version=0.2.0

string TARGET = Argument("target", "Test");

// required
FilePath PROJECT = Argument("project", EnvironmentVariable("IOS_TEST_PROJECT") ?? "");
    string TEST_DEVICE = Argument("device", EnvironmentVariable("IOS_TEST_DEVICE") ?? "ios-simulator-64_14.4"); // comma separated in the form <platform>-<device|simulator>[-<32|64>][_<version>] (eg: ios-simulator-64_13.4,[...])

// optional
var BINLOG = Argument("binlog", EnvironmentVariable("IOS_TEST_BINLOG") ?? PROJECT + ".binlog");
var TEST_APP = Argument("app", EnvironmentVariable("IOS_TEST_APP") ?? "");
var TEST_RESULTS = Argument("results", EnvironmentVariable("IOS_TEST_RESULTS") ?? "");

// other
string PLATFORM = TEST_DEVICE.ToLower().Contains("simulator") ? "iPhoneSimulator" : "iPhone";
string CONFIGURATION = "Release";

Information("Project File: {0}", PROJECT);
Information("Build Binary Log (binlog): {0}", BINLOG);
Information("Build Platform: {0}", PLATFORM);
Information("Build Configuration: {0}", CONFIGURATION);

Task("Build")
    .WithCriteria(!string.IsNullOrEmpty(PROJECT.FullPath))
    .Does(() =>
{
    MSBuild(PROJECT.FullPath, c => {
        c.Configuration = CONFIGURATION;
        c.Restore = true;
        c.Properties["Platform"] = new List<string> { PLATFORM };
        c.Properties["BuildIpa"] = new List<string> { "true" };
        c.Properties["ContinuousIntegrationBuild"] = new List<string> { "false" };
        c.Targets.Clear();
        c.Targets.Add("Rebuild");
        c.BinaryLogger = new MSBuildBinaryLogSettings {
            Enabled = true,
            FileName = BINLOG,
        };
    });
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    if (string.IsNullOrEmpty(TEST_APP)) {
        if (string.IsNullOrEmpty(PROJECT.FullPath))
            throw new Exception("If no app was specified, an app must be provided.");
        var binDir = PROJECT.GetDirectory().Combine("bin").Combine(PLATFORM).Combine(CONFIGURATION).FullPath;
        var apps = GetDirectories(binDir + "/*.app");
        TEST_APP = apps.First().FullPath;
    }
    if (string.IsNullOrEmpty(TEST_RESULTS)) {
        TEST_RESULTS = TEST_APP + "-results";
    }

    Information("Test Device: {0}", TEST_DEVICE);
    Information("Test App: {0}", TEST_APP);
    Information("Test Results Directory: {0}", TEST_RESULTS);

    CleanDirectories(TEST_RESULTS);

    var settings = new DotNetCoreToolSettings
    {
        DiagnosticOutput = true,
        ArgumentCustomization = args=>args.Append("run xharness apple test " +
        $"--app=\"{TEST_APP}\" " +
        $"--targets=\"{TEST_DEVICE}\" " +
        $"--output-directory=\"{TEST_RESULTS}\" " +
        $"--verbosity=\"Debug\" ")
    };

    DotNetCoreTool("tool", settings);
});

RunTarget(TARGET);
