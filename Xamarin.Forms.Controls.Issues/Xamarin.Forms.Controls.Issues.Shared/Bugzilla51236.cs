using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 51236, "[WinRT/UWP] Setting a MasterDetailPage's IsPresented to false should not be allowed in Split mode", PlatformAffected.WinRT)]
	public class Bugzilla51236 : TestMasterDetailPage
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

			Master = new ContentPage
			{
				Title = "Master",
				Content = new StackLayout
				{
					Children =
					{
						listView,
						new Button
						{
							Text = "Set MasterBehavior to Popover",
							Command = new Command(() =>
							{
								MasterBehavior = MasterBehavior.Popover;
								IsPresented = false;
							})
						},
						new Button
						{
							Text = "Set MasterBehavior to Split",
							Command = new Command(() => MasterBehavior = MasterBehavior.Split)
						},
						new Button
						{
							Text = "Set MasterBehavior to SplitOnLandscape",
							Command = new Command(() => MasterBehavior = MasterBehavior.SplitOnLandscape)
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