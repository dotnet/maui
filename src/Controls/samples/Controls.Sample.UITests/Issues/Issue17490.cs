using System;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 17490, "Crash using Pinvoke.SetParent to create Window as Child", PlatformAffected.UWP)]
	public class Issue17490 : TestContentPage
	{
		Label successLabel;
		protected override void Init()
		{
			successLabel = new Label() { Text = "Success", AutomationId = "SuccessLabel" };

			Content = new VerticalStackLayout()
			{
				new Label()
				{
					Text = "This test validates that opening a new WinUI Window parented to this window won't crash."
				}
			};
		}

		protected override void OnNavigatedTo(NavigatedToEventArgs args)
		{
			base.OnNavigatedTo(args);

			try
			{
				var myWindow = new MyWindow(new ContentPage());
				myWindow.Page.Loaded += async (_, _) =>
				{
					await Task.Yield();
					Application.Current.CloseWindow(myWindow);
					await Task.Yield();
					(this.Content as VerticalStackLayout)
						.Add(successLabel);
				};

				Application.Current.OpenWindow(myWindow);
			}
			catch (Exception exc)
			{
				successLabel.Text = $"{exc}";
			}
		}

		public class MyWindow : Window
		{
			public MyWindow(Page page) : base(page)
			{
			}

#if WINDOWS
			protected override void OnHandlerChanged()
			{
				base.OnHandlerChanged();
				if (Handler is null)
				{
					return;
				}

				var mainWindowHandle = (Application.Current.MainPage.Window.Handler.PlatformView as MauiWinUIWindow).GetWindowHandle();
				var childWindowHandle = (Handler.PlatformView as MauiWinUIWindow).GetWindowHandle();

				Platform.PlatformMethods.SetParent(childWindowHandle, mainWindowHandle);
			}
#endif
		}
	}
}
