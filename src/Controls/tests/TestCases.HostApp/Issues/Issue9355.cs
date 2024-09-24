using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 9355, "ScrollViewRenderer renderer dispose crash", PlatformAffected.Android)]
	public class Issue9355 : NavigationPage
	{
		public Issue9355() : base(new MainPage())
		{
		}

		public class MainPage : ContentPage
		{
			const string TestOk = "Test Ok";

			public MainPage()
			{
				Navigation.PushAsync(new ContentPage());
#pragma warning disable CS0618 // Type or member is obsolete
				var stacklayout = new StackLayout
				{
					Children =
				{
					new ScrollView
					{
						HorizontalOptions = LayoutOptions.FillAndExpand,
						VerticalOptions = LayoutOptions.FillAndExpand,
						Orientation = ScrollOrientation.Both,
						Content = new Label
						{
							Text = "Label"
						}
					}
				}
				};
#pragma warning restore CS0618 // Type or member is obsolete

				Microsoft.Maui.Controls.CompressedLayout.SetIsHeadless(stacklayout, true);

				var page = new ContentPage
				{
					Content = stacklayout
				};

#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0612 // Type or member is obsolete
				Device.BeginInvokeOnMainThread(async () =>
				{
					await Navigation.PushModalAsync(page);
					await Navigation.PopModalAsync();

					await Navigation.PushAsync(new ContentPage
					{
						Content = new Label
						{
							AutomationId = TestOk,
							Text = TestOk
						}
					});
				});
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
			}
		}
	}
}