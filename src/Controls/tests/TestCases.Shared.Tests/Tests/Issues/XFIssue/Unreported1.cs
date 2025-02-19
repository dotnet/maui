using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Unreported1 : _IssuesUITest
{
	public Unreported1(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "NRE when switching page on Appearing";

	//[Test]
	//public void Unreported1Test()
	//{
	//	App.Screenshot("ensure there is no crash");
	//}
}