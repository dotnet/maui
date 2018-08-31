using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3343, "[Android] Cursor position in entry and selection length not working on 3.2.0-pre1", PlatformAffected.Android | PlatformAffected.iOS)]
	public class Issue3343 : TestContentPage
	{
		protected override void Init()
		{
			Entry entry = new Entry()
			{
				Text = "Initialized",
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.StartAndExpand,
				WidthRequest = 150
			};

			entry.CursorPosition = 4;
			entry.SelectionLength = entry.Text.Length;

			Label entryLabel = new Label { VerticalOptions = LayoutOptions.Center, FontSize = 8 };
			entryLabel.SetBinding(Label.TextProperty, new Binding(nameof(Entry.CursorPosition), stringFormat: "CursorPosition: {0}", source: entry));

			Label entryLabel2 = new Label { VerticalOptions = LayoutOptions.Center, FontSize = 8 };
			entryLabel2.SetBinding(Label.TextProperty, new Binding(nameof(Entry.SelectionLength), stringFormat: "SelectionLength: {0}", source: entry));

			Entry entry2 = new Entry()
			{
				Text = "Click Button",
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.StartAndExpand,
				WidthRequest = 150
			};

			Label entry2Label = new Label { VerticalOptions = LayoutOptions.Center, FontSize = 8 };
			entry2Label.SetBinding(Label.TextProperty, new Binding(nameof(Entry.CursorPosition), stringFormat: "CursorPosition: {0}", source: entry2));

			Label entry2Label2 = new Label { VerticalOptions = LayoutOptions.Center, FontSize = 8 };
			entry2Label2.SetBinding(Label.TextProperty, new Binding(nameof(Entry.SelectionLength), stringFormat: "SelectionLength: {0}", source: entry2));

			// When the Entry is in a NavPage, the Entry doesn't get first focus on UWP
			string uwp_instructions = Device.RuntimePlatform == Device.UWP ? "Press Tab to focus the first entry. " : "";

			Content = new StackLayout()
			{
				Padding = 20,
				Children =
						{
							new StackLayout{ Children = { entry, entryLabel, entryLabel2  }, Orientation = StackOrientation.Horizontal },
							new StackLayout{ Children = { entry2, entry2Label, entry2Label2 }, Orientation = StackOrientation.Horizontal },
							new Button()
							{
								Text = "Click Me",
								Command = new Command(() =>
								{
									entry2.CursorPosition = 4;
									entry2.SelectionLength = entry2.Text.Length;
								})
							},
							new Button()
							{
								Text = "Click Me After",
								Command = new Command(() =>
								{
									entry2.CursorPosition = 2;
								})
							},
							new Button()
							{
								Text = "Click Me Last",
								Command = new Command(async () =>
								{
									entry2.ClearValue(Entry.SelectionLengthProperty);

									await Task.Delay(500);

									entry2.ClearValue(Entry.CursorPositionProperty);
								})
							},
							new Label{ Text = $"{uwp_instructions}The first Entry should have all text selected starting at character 4. Click the first button to trigger the same selection in the second Entry. Click the second button to move the cursor position but keep the selection length to the end. Click the third button to clear the selection length and then the cursor position." }
						}
			};
		}
	}
}