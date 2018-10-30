using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.Entry)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3385, "[iOS] Entry's TextChanged event is fired on Unfocus even when no text changed", PlatformAffected.iOS)]
	public class Issue3385 : TestContentPage
	{
		Label _label = new Label { Text = "Focus the Entry, then click the Label to unfocus the Entry. If this text changes, this test has failed." };
		protected override void Init()
		{
			var entry = new Entry { AutomationId = "entry" };
			entry.TextChanged += Entry_TextChanged;

			var layout = new StackLayout { Children = { _label, entry, new Label { Text = "Click me", AutomationId = "click" } } };

			Content = layout;
		}

		void Entry_TextChanged(object sender, TextChangedEventArgs e)
		{
			_label.Text = "FAIL";
		}

#if UITEST
		[Test]
		public void Issue3385Test()
		{
			RunningApp.WaitForElement("entry");
			RunningApp.Tap("entry");
			RunningApp.WaitForElement("click");
			RunningApp.Tap("click");
			RunningApp.WaitForNoElement("FAIL");
		}
#endif
	}
}