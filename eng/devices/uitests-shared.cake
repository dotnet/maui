#load "../cake/helpers.cake"

if (!IsCIBuild() && GetBuildVariable("workloads", "notset") == "notset")
{
	SetEnvironmentVariable("workloads", "global");
}

#load "../cake/dotnet.cake"
#load "./devices-shared.cake"

bool deviceBoot = Argument("boot", TARGET.ToLower() != "uitest-build");
bool targetBoot = TARGET.ToLower() == "boot";
bool targetCleanup = TARGET.ToLower() == "cleanup";
bool deviceBootWait = Argument("wait", true);