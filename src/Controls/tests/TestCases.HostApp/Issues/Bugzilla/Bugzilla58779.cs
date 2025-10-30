namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 58779, "[MacOS] DisplayActionSheet on MacOS needs scroll bars if list is long", PlatformAffected.All)]
public class Bugzilla58779 : TestContentPage
{
	const string ButtonId = "button";
	const string CancelId = "cancel";

	protected override void Init()
	{
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0612 // Type or member is obsolete
		Button button = new Button
		{
			Text = "Click Here",
			FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Button), false),
			BorderWidth = 1,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.CenterAndExpand,
			AutomationId = ButtonId,
		};
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete

		// The root page of your application
		var content = new StackLayout
		{
			VerticalOptions = LayoutOptions.Center,
			Children = {
				new Label {
					HorizontalTextAlignment = TextAlignment.Center,
					Text = "Tap on the button to show the DisplayActionSheet with 15 items"
				},
				new Label {
					HorizontalTextAlignment = TextAlignment.Center,
					Text = "The list of items should be scrollable and Cancel should be visible"
				},
				button

			}
		};

		button.Clicked += (sender, e) =>
		{
			String[] string_array = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15" };
			this.DisplayActionSheetAsync("title", CancelId, "destruction", string_array);
		};

		Content = content;
	}
}