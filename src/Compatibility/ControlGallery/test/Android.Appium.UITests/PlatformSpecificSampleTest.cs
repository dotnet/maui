using NUnit.Framework;

namespace UITests;

public class PlatformSpecificSampleTest : UITestBase
{
	[Test]
	public void SampleTest()
	{
		App.GetScreenshot().SaveAsFile($"{nameof(SampleTest)}.png");
	}
}