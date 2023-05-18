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
	[Issue(IssueTracker.Bugzilla, 51236, "[WinRT/UWP] Setting a FlyoutPage's IsPresented to false should not be allowed in Split mode", PlatformAffected.WinRT)]
	public class Bugzilla51236 : TestFlyoutPage
	{
		protected override void Init()
		{
			var listView = new ListView
			{
				ItemsSource = new string[] { "A", "B", "C" },
				ItemTemplate = new DataTemplate(() =>
				{
					var cell = new ViewCell();
					cell.View = new StackLayout
					{
						Children =
						{
							new Label { Text = "Click to set IsPresented to false" }
						}
					};
					return cell;
				}),
			};

			IsPresented = false;
			listView.ItemTapped += (s, e) =>
			{
				IsPresented = false;
				listView.SelectedItem = null;
				Detail = new ContentPage
				{
					Title = "Detail",
					Content = new StackLayout
					{
						Children =
						{
							new Button
							{
								Text = "Set IsPresented to true",
								Command = new Command(() => IsPresented = true)
							}
						}
					}
				};
			};

			Flyout = new ContentPage
			{
				Title = "Flyout",
				Content = new StackLayout
				{
					Children =
					{
						listView,
						new Button
						{
							Text = "Set FlyoutLayoutBehavior to Popover",
							Command = new Command(() =>
							{
								FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;
								IsPresented = false;
							})
						},
						new Button
						{
							Text = "Set FlyoutLayoutBehavior to Split",
							Command = new Command(() => FlyoutLayoutBehavior = FlyoutLayoutBehavior.Split)
						},
						new Button
						{
							Text = "Set FlyoutLayoutBehavior to SplitOnLandscape",
							Command = new Command(() => FlyoutLayoutBehavior = FlyoutLayoutBehavior.SplitOnLandscape)
						},
					}
				}
			};

			Detail = new ContentPage
			{
				Title = "Detail",
				Content = new StackLayout
				{
					Children =
					{
						new Button
						{
							Text = "Set IsPresented to true",
							Command = new Command(() => IsPresented = true)
						}
					}
				}
			};
		}
	}
}