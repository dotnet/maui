namespace Microsoft.Maui.AppleIntegrationTests;

static class Categories
{
	// this is a special job that runs on the samples
	public const string Samples = nameof(Samples);

	// these are special run on "device" jobs
	public const string RunOnAndroid = nameof(RunOnAndroid);
	public const string RunOniOS = nameof(RunOniOS);

	// these are normal jobs
	public const string WindowsTemplates = nameof(WindowsTemplates);
	public const string macOSTemplates = nameof(macOSTemplates);
	public const string Build = nameof(Build);
	public const string Blazor = nameof(Blazor);
	public const string MultiProject = nameof(MultiProject);
	public const string AOT = nameof(AOT);
}
