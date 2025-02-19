using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest.Queries;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5367, "[Bug] Editor with MaxLength", PlatformAffected.Android)]
#if UITEST
	[Category(UITestCategories.Editor)]
#endif
	public class Issue5367 : TestContentPage
	{
		const string MaxLengthEditor = "MaxLength Editor";
		const string ForceBigStringButton = "Force Big String Button";

		protected override void Init()
		{
			var maxLength = 14;
			var editor = new Editor()
			{
				AutomationId = MaxLengthEditor,
				Text = $"MaxLength = {maxLength}",
				MaxLength = maxLength,
			};

			Content = new ScrollView()
			{
				Content = new StackLayout()
				{
					Children =
					{
						editor,
						new Button()
						{
							AutomationId = ForceBigStringButton,
							Text = "Force editor text greater than maxlength",
							Command = new Command(()=> editor.Text += $"This should not appear on editor")
						}
					}
				}
			};
		}

#if UITEST
		[Test]
		[UiTest(typeof(Editor))]
		[Compatibility.UITests.FailsOnMauiIOS]
		[Compatibility.UITests.FailsOnMauiAndroid]
		public void Issue5367TestMaxLengthCrashesApp()
		{
			RunningApp.WaitForElement(MaxLengthEditor);
			RunningApp.Tap(ForceBigStringButton);
		}
#endif
	}
}
