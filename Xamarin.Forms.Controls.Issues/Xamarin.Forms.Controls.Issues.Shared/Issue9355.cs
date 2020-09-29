using NUnit.Framework;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 9355, "ScrollViewRenderer renderer dispose crash", PlatformAffected.Android)]
	public class Issue9355 : TestNavigationPage
	{
		const string TestOk = "Test Ok";

		protected override void Init()
		{
			PushAsync(new ContentPage());
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

			Forms.CompressedLayout.SetIsHeadless(stacklayout, true);

			var page = new ContentPage
			{
				Content = stacklayout
			};

			Device.BeginInvokeOnMainThread(async () =>
			{
				await Navigation.PushModalAsync(page);
				await Navigation.PopModalAsync();

				await PushAsync(new ContentPage
				{
					Content = new Label
					{
						Text = TestOk
					}
				});
			});
		}

#if UITEST
		[Test]
		public void Issue9355Test()
		{
			RunningApp.WaitForElement(TestOk);
		}
#endif
	}
}