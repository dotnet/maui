namespace Microsoft.Maui.IntegrationTests;

static class Categories
{
	// this is a special job that runs on the samples
	public const string Samples = nameof(Samples);

	// these are special run on "device" jobs
	public const string RunOnAndroid = nameof(RunOnAndroid);
	public const string RunOniOS = nameof(RunOniOS);

	// Individual RunOniOS test categories for parallel execution
	public const string RunOniOS_MauiDebug = nameof(RunOniOS_MauiDebug);
	public const string RunOniOS_MauiRelease = nameof(RunOniOS_MauiRelease);
	public const string RunOniOS_MauiReleaseTrimFull = nameof(RunOniOS_MauiReleaseTrimFull);
	public const string RunOniOS_BlazorDebug = nameof(RunOniOS_BlazorDebug);
	public const string RunOniOS_BlazorRelease = nameof(RunOniOS_BlazorRelease);
	public const string RunOniOS_BlazorReleaseTrimFull = nameof(RunOniOS_BlazorReleaseTrimFull);
	public const string RunOniOS_MauiNativeAOT = nameof(RunOniOS_MauiNativeAOT);

	// these are normal jobs
	public const string WindowsTemplates = nameof(WindowsTemplates);
	public const string macOSTemplates = nameof(macOSTemplates);
	public const string Build = nameof(Build);
	public const string Blazor = nameof(Blazor);
	public const string MultiProject = nameof(MultiProject);
	public const string AOT = nameof(AOT);
}
