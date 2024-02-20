using NUnit.Framework;

namespace UITests;

public class PlatformSpecificSampleTest : BaseTest
{
	[Test]
	public void SampleTest()
	{
		App.GetScreenshot().SaveAsFile($"{nameof(SampleTest)}.png");
	}
}