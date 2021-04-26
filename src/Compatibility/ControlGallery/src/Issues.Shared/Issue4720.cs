using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using Microsoft.Maui.Graphics;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.WebView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 4720, "UWP: Webview: Memory Leak in WebView", PlatformAffected.UWP)]
	public class Issue4720 : TestNavigationPage
	{
		protected override void Init()
		{
			PushAsync(new Issue4720Content());
		}

#if UITEST
		protected override bool Isolate => true;

		[Test]
		public void WebViewDoesntCrashWhenLoadingAHeavyPageAndUsingExecutionModeSeparateProcess()
		{
			//4 iterations were enough to run out of memory before the fix.
			int iterations = 10;

			for (int n = 0; n < iterations; n++)
			{
				RunningApp.WaitForElement(q => q.Marked("New Page"));
				RunningApp.Tap(q => q.Marked("New Page"));
				RunningApp.WaitForElement(q => q.Marked("Close Page"));
				Thread.Sleep(250);
				RunningApp.Tap(q => q.Marked("Close Page"));
			}
			RunningApp.Tap(q => q.Marked("GC"));
		}

#endif

		[Preserve(AllMembers = true)]
		public class Issue4720WebPage : ContentPage
		{
			static int s_count;
			WebView _webView;

			public Issue4720WebPage()
			{
				Interlocked.Increment(ref s_count);
				Debug.WriteLine($"++++++++ Issue4720WebPage : Constructor, count is {s_count}");

				var label = new Label { Text = "Test case for GitHub issue #4720." };

				var button = new Button { Text = "Close Page" };
				button.Clicked += ClosePageClicked;


				var btnChangeExecutionMode = new Button { Text = "Change Execution Mode" };
				btnChangeExecutionMode.Clicked += ChangeExecutionModeClicked;

				_webView = new WebView()
				{
					Source = new UrlWebViewSource { Url = "https://www.microsoft.com/" },
					HorizontalOptions = LayoutOptions.FillAndExpand,
					VerticalOptions = LayoutOptions.FillAndExpand,
					BackgroundColor = Colors.Red

				};

				_webView.On<PlatformConfiguration.Windows>().SetExecutionMode(WebViewExecutionMode.SeparateProcess);

				Content = new StackLayout { Children = { label, button, btnChangeExecutionMode, _webView } };
			}

			async void ClosePageClicked(object sender, EventArgs e)
			{
				await Navigation.PopAsync();
			}

			void ChangeExecutionModeClicked(object sender, EventArgs e)
			{
				if (_webView.On<PlatformConfiguration.Windows>().GetExecutionMode() == WebViewExecutionMode.SameThread)
					_webView.On<PlatformConfiguration.Windows>().SetExecutionMode(WebViewExecutionMode.SeparateProcess);
				else
					_webView.On<PlatformConfiguration.Windows>().SetExecutionMode(WebViewExecutionMode.SameThread);
			}

			~Issue4720WebPage()
			{
				Interlocked.Decrement(ref s_count);
				Debug.WriteLine($"-------- Issue4720WebPage: Destructor, count is {s_count}");
			}
		}

		[Preserve(AllMembers = true)]
		public class Issue4720Content : ContentPage
		{
			static int s_count;

			public Issue4720Content()
			{
				Interlocked.Increment(ref s_count);
				Debug.WriteLine($">>>>> Issue4720Content Issue4720Content : Constructor, count is {s_count}");

				var button = new Button { Text = "New Page" };
				button.Clicked += Button_Clicked;

				var gcbutton = new Button { Text = "GC" };
				gcbutton.Clicked += GCbutton_Clicked;

				var instructions = new Label() { Text = "Navigate forward and back multiple times. If you don't see any out of memory exceptions the test has passed." };
				Content = new StackLayout { Children = { button, gcbutton, instructions } };
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
}
