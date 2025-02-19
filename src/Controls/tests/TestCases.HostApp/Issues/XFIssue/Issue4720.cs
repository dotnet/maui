using System.Diagnostics;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 4720, "UWP: Webview: Memory Leak in WebView", PlatformAffected.UWP)]
public class Issue4720 : TestNavigationPage
{
	protected override void Init()
	{
		PushAsync(new Issue4720Content());
	}

	public class Issue4720WebPage : ContentPage
	{
		static int s_count;
		Microsoft.Maui.Controls.WebView _webView;

		public Issue4720WebPage()
		{
			Interlocked.Increment(ref s_count);
			Debug.WriteLine($"++++++++ Issue4720WebPage : Constructor, count is {s_count}");

			var label = new Microsoft.Maui.Controls.Label { Text = "Test case for GitHub issue #4720." };

			var button = new Button { Text = "Close Page", AutomationId = "ClosePage" };
			button.Clicked += ClosePageClicked;


			var btnChangeExecutionMode = new Button { Text = "Change Execution Mode" };
			btnChangeExecutionMode.Clicked += ChangeExecutionModeClicked;

#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
			_webView = new Microsoft.Maui.Controls.WebView()
			{
				Source = new UrlWebViewSource { Url = "https://www.microsoft.com/" },
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
				BackgroundColor = Colors.Red

			};
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete

			_webView.On<Microsoft.Maui.Controls.PlatformConfiguration.Windows>().SetExecutionMode(WebViewExecutionMode.SeparateProcess);

			var stackLayout = new StackLayout
			{
				label,
				button,
				btnChangeExecutionMode,
				_webView
			};
			Content = stackLayout;
		}

		async void ClosePageClicked(object sender, EventArgs e)
		{
			await Navigation.PopAsync();
		}

		void ChangeExecutionModeClicked(object sender, EventArgs e)
		{
			if (_webView.On<Microsoft.Maui.Controls.PlatformConfiguration.Windows>().GetExecutionMode() == WebViewExecutionMode.SameThread)
				_webView.On<Microsoft.Maui.Controls.PlatformConfiguration.Windows>().SetExecutionMode(WebViewExecutionMode.SeparateProcess);
			else
				_webView.On<Microsoft.Maui.Controls.PlatformConfiguration.Windows>().SetExecutionMode(WebViewExecutionMode.SameThread);
		}

		~Issue4720WebPage()
		{
			Interlocked.Decrement(ref s_count);
			Debug.WriteLine($"-------- Issue4720WebPage: Destructor, count is {s_count}");
		}
	}

	public class Issue4720Content : ContentPage
	{
		static int s_count;

		public Issue4720Content()
		{
			Interlocked.Increment(ref s_count);
			Debug.WriteLine($">>>>> Issue4720Content Issue4720Content : Constructor, count is {s_count}");

			var button = new Button { Text = "New Page", AutomationId = "NewPage" };
			button.Clicked += Button_Clicked;

			var gcbutton = new Button { Text = "GC", AutomationId = "GC" };
			gcbutton.Clicked += GCbutton_Clicked;

			var instructions = new Microsoft.Maui.Controls.Label() { Text = "Navigate forward and back multiple times. If you don't see any out of memory exceptions the test has passed." };

			var stackLayout = new StackLayout
			{
				button,
				gcbutton,
				instructions
			};

			Content = stackLayout;
		}

		void GCbutton_Clicked(object sender, EventArgs e)
		{
			System.Diagnostics.Debug.WriteLine(">>>>>>>> Running Garbage Collection");
			GarbageCollectionHelper.Collect();
			System.Diagnostics.Debug.WriteLine($">>>>>>>> GC.GetTotalMemory = {GC.GetTotalMemory(true):n0}");
		}

		void Button_Clicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new Issue4720WebPage());
		}

		~Issue4720Content()
		{
			Interlocked.Decrement(ref s_count);
			Debug.WriteLine($">>>>> Issue4720Content ~Issue4720Content : Destructor, count is {s_count}");
		}
	}
}
