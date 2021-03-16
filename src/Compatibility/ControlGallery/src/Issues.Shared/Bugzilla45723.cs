using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 45723, "Entry / Editor and a Button. Tapping the button dismisses the keyboard", PlatformAffected.iOS)]
	public class Bugzilla45723 : TestNavigationPage // or TestFlyoutPage, etc ...
	{
		protected override void Init()
		{
			PushAsync(new EditorAndButtonReproPage());
		}
	}

	public class EditorAndButtonReproPage : ContentPage
	{
		public EditorAndButtonReproPage()
		{
			BackgroundColor = Color.Gray;
			Padding = 50;
			var editor = new Editor { HorizontalOptions = LayoutOptions.FillAndExpand };
			var editorButton = new Button { Text = "OK", HorizontalOptions = LayoutOptions.End };
			editorButton.On<iOS>().SetCanBecomeFirstResponder(true);
			var editorLayout = new StackLayout { Orientation = StackOrientation.Horizontal, Children = { editor, editorButton }, VerticalOptions = LayoutOptions.Start };
			var endtry = new Entry { Placeholder = "Entry", HorizontalOptions = LayoutOptions.FillAndExpand };
			var entryButton = new Button { Text = "OK", HorizontalOptions = LayoutOptions.End };
			var entryLayout = new StackLayout { Orientation = StackOrientation.Horizontal, Children = { endtry, entryButton }, VerticalOptions = LayoutOptions.Start };
			Content = new StackLayout
			{
				Children = {
					new Label() { Text = "Click editor to make keyboard appear. Click ok and keyboard shouldn't disappear"},
					editorLayout,
					new Label() { Text = "Click entry to make keyboard appear. Click ok and keyboard should disappear"},
					entryLayout
				}
			};
		}
	}
}