using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 6713, "[Enhancement] Display prompts", PlatformAffected.iOS | PlatformAffected.Android)]
	public class Issue6713 : TestContentPage // or TestMasterDetailPage, etc ...
	{
		protected override void Init()
		{
			var scrollView = new ScrollView();

			var stackLayout = new StackLayout
			{
				Orientation = StackOrientation.Vertical,
				Spacing = 10,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand
			};

			var button = new Button { Text = "Default keyboard" };
			button.Clicked += async (sender, e) =>
			{
				var result = await DisplayPromptAsync("What’s the most useless product around today?", "The USB pet rock is definitely up there. What items do you have a hard time believing they actually exist?");

				if (result != null)
					(sender as Button).Text = result;
			};
            stackLayout.Children.Add(button);

            var button2 = new Button { Text = "Numeric keyboard" };
            button2.Clicked += async (sender, e) =>
            {
	            var result = await DisplayPromptAsync("What’s the meaning of life?", "You know that number.", maxLength:2, keyboard:Keyboard.Numeric);

	            if (result != null)
					(sender as Button).Text = result;
			};
            stackLayout.Children.Add(button2);

			scrollView.Content = stackLayout;
			Content = scrollView;
		}
	}
}