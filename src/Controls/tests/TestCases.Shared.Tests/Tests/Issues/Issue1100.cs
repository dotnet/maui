using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue1100 : _IssuesUITest
{
	public Issue1100(TestDevice testDevice) : base(testDevice)
	{
	}
	public override string Issue => "Add DateOnly and TimeOnly converters to DatePicker and TimePicker";

	[Test]
	[Category(UITestCategories.DatePicker)]
	public void DatePickerConverters()
	{
		Assert.That(App.WaitForElement("LabelDateOnly").GetText(), Is.EqualTo("DateOnly Value: 2025-07-28"));
		Assert.That(App.WaitForElement("LabelDateTime").GetText(), Is.EqualTo("DateTime Value: 2025-07-28"));
		Assert.That(App.WaitForElement("LabelString").GetText(), Is.EqualTo("String Value: 2025-07-28"));
		Assert.That(App.WaitForElement("LabelMinMax").GetText(), Is.EqualTo("MinMax DatePicker: Min=2025-07-01, Max=2025-08-31"));
	}

	[Test]
	[Category(UITestCategories.TimePicker)]
	public void TimePickerConverters()
	{
		Assert.That(App.WaitForElement("LabelTimeOnly").GetText(), Is.EqualTo("TimeOnly Value: 10:30:00"));
		Assert.That(App.WaitForElement("LabelTimeSpan").GetText(), Is.EqualTo("TimeSpan Value: 10:30:00"));
		Assert.That(App.WaitForElement("LabelTimeString").GetText(), Is.EqualTo("Time String Value: 10:30:00"));
	}
}