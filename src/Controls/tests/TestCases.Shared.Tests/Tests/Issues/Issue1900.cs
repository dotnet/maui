#if TEST_FAILS_ON_WINDOWS //On Windows App crashes on CI, but it does not replicate locally. Adding failure for Windows to ensure CI stability.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue1900 : _IssuesUITest
	{
		public Issue1900(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Xamarin ios ListView ObservableCollection<myClass>. Collection.Add() throwing 'Index # is greater than the number of rows #' exception";

		[Test]
		[Category(UITestCategories.ListView)]
		public void Issue1900Test()
		{
			App.WaitForElement("ListView");
		}
	}
}
#endif