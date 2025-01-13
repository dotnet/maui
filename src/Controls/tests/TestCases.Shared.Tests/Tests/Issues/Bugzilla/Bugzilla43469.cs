using NUnit.Framework;
using UITest.Appium;
using UITest.Core;
 
namespace Microsoft.Maui.TestCases.Tests.Issues;
 
public class Bugzilla43469 : _IssuesUITest
{
#if MACCATALYST
	const string CancelBtn = "action-button--999";
#else
	const string CancelBtn = "Cancel";
#endif
    public Bugzilla43469(TestDevice testDevice) : base(testDevice)
    {
    }
 
    public override string Issue => "Calling DisplayAlert twice in WinRT causes a crash";
 
    [Test]
    [Category(UITestCategories.DisplayAlert)]
    public void Bugzilla43469Test()
    {
        App.WaitForElement("kButton");
        App.Tap("kButton");
        App.WaitForElementTillPageNavigationSettled("First");
        App.Tap(CancelBtn);
        App.WaitForElementTillPageNavigationSettled("Second");
        App.Tap(CancelBtn);
        App.WaitForElementTillPageNavigationSettled("Three");
        App.Tap(CancelBtn);
#if !MACCATALYST // Test fails on Catalyst platforms because the alert box cannot be opened propely 6 times when invoke this on using BeginInvokeOnMainThread Issue: https://github.com/dotnet/maui/issues/26481
		for(int i=0; i<3; i++)
		{
			TapCancel(CancelBtn);
		}
#endif
        App.WaitForElement("kButton");
    }
    void TapCancel(string CancelBtn)
    {
    	App.WaitForElementTillPageNavigationSettled(CancelBtn);
    	App.Tap(CancelBtn);
    }
}