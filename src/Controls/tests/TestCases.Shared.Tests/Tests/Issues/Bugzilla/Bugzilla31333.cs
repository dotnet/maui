using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.TableView)]
public class Bugzilla31333 : _IssuesUITest
{
	public Bugzilla31333(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Focus() on Entry in ViewCell brings up keyboard, but doesn't have cursor in EditText";

	[Test]
	public void Issue31333FocusEntryInListViewCell()
	{
		App.WaitForElement("Focus Entry in ListView");
		App.Tap("Focus Entry in ListView");
		App.EnterText("EntryListView", "Entry in ListView Success");
		Assert.That(App.WaitForElement("EntryListView")?.GetText(), Is.EqualTo("Entry in ListView Success"));
		App.Tap("Focus Entry in ListView");
	}

	[Test]
	public void Issue31333FocusEditorInListViewCell()
	{
		App.WaitForElement("Focus Editor in ListView");
		App.Tap("Focus Editor in ListView");
		App.EnterText("EditorListView", "Editor in ListView Success");
		Assert.That(App.WaitForElement("EditorListView")?.GetText(), Is.EqualTo("Editor in ListView Success"));
		App.Tap("Focus Editor in ListView");
	}

	[Test]
	public void Issue31333FocusEntryInTableViewCell()
	{
		App.WaitForElement("Focus Entry in Table");
		App.Tap("Focus Entry in Table");
		App.EnterText("EntryTable", "Entry in TableView Success");
		Assert.That(App.WaitForElement("EntryTable")?.GetText(), Is.EqualTo("Entry in TableView Success"));
		App.Tap("Focus Entry in Table");
	}
#if TEST_FAILS_ON_IOS //Once Editor text is entered the cursor move to second line when using App.EnterText method in appium which results retrived text is not as expected one. 
	[Test]
	public void Issue31333FocusEditorInTableViewCell()
	{
		App.WaitForElement("Focus Editor in Table");
		App.Tap("Focus Editor in Table");
		App.EnterText("EditorTable", "Editor in TableView Success");
		Assert.That(App.WaitForElement("EditorTable")?.GetText(), Is.EqualTo("Editor in TableView Success"));
		App.Tap("Focus Editor in Table");
	}
#endif
}
