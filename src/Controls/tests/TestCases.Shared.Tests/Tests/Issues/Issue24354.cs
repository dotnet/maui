using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
 public class Issue24354 : _IssuesUITest
 {
  public Issue24354(TestDevice testDevice) : base(testDevice) { }

  public override string Issue => "[Android] Grid ColumnSpacing affects child's scrollview content size";

  [Test]
  [Category(UITestCategories.Layout)]
  public void GridColumnSpacingDoesNotAffectChildScrollViewContentSize()
  {
   App.WaitForElement("ScrollView");
   VerifyScreenshot();
  }
 }
}