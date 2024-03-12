using System.Drawing;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues;

public class Issue20294 : _IssuesUITest
{
	public Issue20294(TestDevice device) : base(device) { }

	public override string Issue => "CollectionView containing a Footer and a Border with StrokeThickness set to decimal value crashes on scroll";

	[Test]
	public void ScrollToEndDoesntCrash()
	{
 		App.ScrollDown("FOOTER");
		App.ScrollUp("ONE");
	}
}
