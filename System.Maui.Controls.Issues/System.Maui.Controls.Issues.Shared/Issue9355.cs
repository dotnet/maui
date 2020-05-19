using NUnit.Framework;
using System.Maui.CustomAttributes;
using System.Maui.Internals;

namespace System.Maui.Controls.Issues
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

			System.Maui.CompressedLayout.SetIsHeadless(stacklayout, true);

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