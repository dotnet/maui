using Xunit;
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

	[Fact]
	public void Issue31333FocusEntryInListViewCell()
	{
		App.WaitForElement("Focus Entry in ListView");
		App.Tap("Focus Entry in ListView");
		App.EnterText("EntryListView", "Entry in ListView Success");
		Assert.Equal("Entry in ListView Success", App.WaitForElement("EntryListView")?.GetText());
		App.Tap("Focus Entry in ListView");
	}

	[Fact]
	public void Issue31333FocusEditorInListViewCell()
	{
		App.WaitForElement("Focus Editor in ListView");
		App.Tap("Focus Editor in ListView");
		App.EnterText("EditorListView", "Editor in ListView Success");
		Assert.Equal("Editor in ListView Success", App.WaitForElement("EditorListView")?.GetText());
		App.Tap("Focus Editor in ListView");
	}

	[Fact]
	public void Issue31333FocusEntryInTableViewCell()
	{
		App.WaitForElement("Focus Entry in Table");
		App.Tap("Focus Entry in Table");
		App.EnterText("EntryTable", "Entry in TableView Success");
		Assert.Equal("Entry in TableView Success", App.WaitForElement("EntryTable")?.GetText());
		App.Tap("Focus Entry in Table");
	}
#if TEST_FAILS_ON_IOS //Once Editor text is entered the cursor move to second line when using App.EnterText method in appium which results retrived text is not as expected one. 
	[Fact]
	public void Issue31333FocusEditorInTableViewCell()
	{
		App.WaitForElement("Focus Editor in Table");
		App.Tap("Focus Editor in Table");
		App.EnterText("EditorTable", "Editor in TableView Success");
		Assert.Equal("Editor in TableView Success", App.WaitForElement("EditorTable")?.GetText());
		App.Tap("Focus Editor in Table");
	}
#endif
}
