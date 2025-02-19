using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 38723, "Update Content in Picker's SelectedIndexChanged event causes NullReferenceException", PlatformAffected.All)]
	public class Bugzilla38723 : TestContentPage
	{
		protected override void Init()
		{
			var label = new Label
			{
				Text = "NoSelected"
			};

			var picker = new Picker
			{
				Title = "Options",
				ItemsSource = new[] { "option1", "option2", "option3" }
			};

			picker.SelectedIndexChanged += (sender, args) =>
			{
				label.Text = "Selected";
				Content = label;
			};

			var button = new Button
			{
				Text = "SELECT"
			};

			button.Clicked += (sender, args) =>
			{
				picker.SelectedIndex = 0;
			};

			Content = new StackLayout
			{
				Children =
				{
					label,
					picker,
					button
				}
			};
		}

#if UITEST
[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnMauiAndroid]
[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnMauiIOS]
		[Test]
		public void Bugzilla38723Test()
		{
			RunningApp.Tap(q => q.Marked("SELECT"));
			RunningApp.WaitForElement(q => q.Marked("Selected"));
			RunningApp.WaitForNoElement(q => q.Marked("SELECT"));
		}
#endif
	}
}
