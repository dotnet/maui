using System;
using System.Diagnostics;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.None, 0, "Navigating Between Transient Shell States Recreates Pages")]
	public class ShellTransientTests : Shell
	{
        List<ContentPage> _contentPages = new List<ContentPage>();
		protected override void OnNavigated(ShellNavigatedEventArgs args)
		{
			base.OnNavigated(args);

            if (_contentPages.Contains(this.CurrentPage))
            {
                (CurrentPage as ContentPage).Content = new VerticalStackLayout()
                {
                    Children =
                    {
                        new Label { Text = "Test Failed I am not a new page", AutomationId = "Failure" }
                    }
                };
            }
            else
            {
                (CurrentPage as ContentPage).Content = new VerticalStackLayout()
                {
                    Children =
                    {
                        new Label { Text = "I am a new page", AutomationId = "Success" }
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
                ContentTemplate = new DataTemplate(typeof(TransientPage))
            };

            var shellContent3 = new ShellContent()
            {
                ContentTemplate = new DataTemplate(typeof(ContentPage))
            };

            Items.Add(new FlyoutItem()
            {
                Title = "Flyout Item 1",
                Items =
                {
                    shellContent1
                }
            });

            Items.Add(new FlyoutItem()
            {
                Title = "Flyout Item 2",
                Items =
                {
                    shellContent2
                }
            });


            Items.Add(new FlyoutItem()
            {
                Title = "Flyout Item 3",
                Items =
                {
                    shellContent3
                }
            });
        }
	}
}