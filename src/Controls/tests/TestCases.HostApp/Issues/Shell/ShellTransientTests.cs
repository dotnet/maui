using System;
using System.Diagnostics;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.None, 0, "Validate Basic Service Lifetime Behavior On Shell")]
	public class ShellTransientTests : Shell
	{
		static List<ContentPage> _contentPages = new List<ContentPage>();
		protected override void OnNavigated(ShellNavigatedEventArgs args)
		{
			base.OnNavigated(args);
			LoadCurrentPage();
		}

		void LoadCurrentPage()
		{
			var navigatetoTransientPage = new Button
			{
				Text = "Navigate to transient page",
				AutomationId = "NavigateToTransientPage",
				Command = new Command(() =>
				{
					((ContentPage)this.CurrentPage).Content = new Label() { Text = "Navigating. If you tried to navigate to the same page type, you'll be stuck here." };
					this.CurrentItem = Items[0];
				})
			};

			var navigateToNotRegisteredPage = new Button
			{
				Text = "Navigate to Unregistered page",
				AutomationId = "NavigateToUnregisteredPage",
				Command = new Command(() =>
				{
					((ContentPage)this.CurrentPage).Content = new Label() { Text = "Navigating. If you tried to navigate to the same page type, you'll be stuck here." };
					this.CurrentItem = Items[1];
				})
			};

			var navigateToScopedPage = new Button
			{
				Text = "Navigate to scoped page",
				AutomationId = "NavigateToScopedPage",
				Command = new Command(() =>
				{
					((ContentPage)this.CurrentPage).Content = new Label() { Text = "Navigating. If you tried to navigate to the same page type, you'll be stuck here." };
					this.CurrentItem = Items[2];
				})
			};

			var navigateToNewShell = new Button
			{
				Text = "Navigate to New Shell",
				AutomationId = "NavigateToNewShell",
				Command = new Command(() =>
				{
					this.Window.Page = new ShellTransientTests();
				})
			};

			if (_contentPages.Contains(this.CurrentPage))
			{
				(CurrentPage as ContentPage).Content = new VerticalStackLayout()
				{
					Children =
					{
						navigatetoTransientPage,
						navigateToNotRegisteredPage,
						navigateToScopedPage,
						navigateToNewShell,
						new Label { Text = "I am not a new page", AutomationId = "OldPage" }
					}
				};
			}
			else
			{
				(CurrentPage as ContentPage).Content = new VerticalStackLayout()
				{
					Children =
					{
						navigatetoTransientPage,
						navigateToNotRegisteredPage,
						navigateToScopedPage,
						navigateToNewShell,
						new Label { Text = "I am a new page", AutomationId = "NewPage" }
					}
				};
			}

			_contentPages.Add((ContentPage)this.CurrentPage);
		}

		public ShellTransientTests()
		{
			var shellContent1 = new ShellContent()
			{
				ContentTemplate = new DataTemplate(typeof(TransientPage))
			};

			var shellContent2 = new ShellContent()
			{
				ContentTemplate = new DataTemplate(typeof(ContentPage))
			};

			var shellContent3 = new ShellContent()
			{
				ContentTemplate = new DataTemplate(typeof(ScopedPage))
			};

			Items.Add(new FlyoutItem()
			{
				Title = "Transient Page",
				Items =
				{
					shellContent1
				}
			});

			Items.Add(new FlyoutItem()
			{
				Title = "Not Registered Page",
				Items =
				{
					shellContent2
				}
			});

			Items.Add(new FlyoutItem()
			{
				Title = "Scoped Page",
				Items =
				{
					shellContent3
				}
			});
		}
	}
}