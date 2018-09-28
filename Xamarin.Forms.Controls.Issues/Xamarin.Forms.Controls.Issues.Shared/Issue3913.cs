using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3913, "[WPF] Height of items not consistent", PlatformAffected.WPF)]
	public class Issue3913 : TestContentPage
	{
		readonly StackLayout stackLayout = new StackLayout();
		readonly Label label = new Label()
		{
			Margin = new Thickness(5),
			Text = "Hello from Xamarin Forms",
			BackgroundColor = Color.Red,
			VerticalOptions = LayoutOptions.StartAndExpand,
			HeightRequest = 100,
			HorizontalOptions = LayoutOptions.Fill
		};

		protected override void Init()
		{
			Button showButton = new Button();
			showButton.Text = "Show";
			showButton.Clicked += ShowButton_Clicked;

			Button hideButton = new Button();
			hideButton.Text = "Hide";
			hideButton.Clicked += HideButton_Clicked;

			stackLayout.Children.Add(showButton);
			stackLayout.Children.Add(hideButton);
			Content = stackLayout;
		}

		private void ShowButton_Clicked(object sender, System.EventArgs e)
		{
			stackLayout.Children.Add(label);
		}

		private void HideButton_Clicked(object sender, System.EventArgs e)
		{
			stackLayout.Children.Remove(label);
		}
	}
}