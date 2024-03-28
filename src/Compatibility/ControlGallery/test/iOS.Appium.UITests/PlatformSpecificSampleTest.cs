using NUnit.Framework;

namespace UITests;

public class PlatformSpecificSampleTest : UITest
{
	public PlatformSpecificSampleTest(TestDevice testDevice) : base(testDevice)
	{
	}

	[Test]
	public void SampleTest()
	{
		Driver?.GetScreenshot().SaveAsFile($"{nameof(SampleTest)}.png");
	}
}