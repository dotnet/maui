using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIView;
#elif __ANDROID__
using PlatformView = Android.Views.View;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
#elif TIZEN
using PlatformView = Tizen.NUI.BaseComponents.View;
#elif (NETSTANDARD || !PLATFORM)
using PlatformView = System.Object;
#endif

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 24532, "Rendering performance", PlatformAffected.All)]
public partial class Issue24532 : ContentPage
{
	public List<Model> Models { get; set; }

	public Issue24532()
	{
		Models = GenerateOneItem("Test0");
		BindingContext = this;
		InitializeComponent();
	}

	private async void ButtonClicked(object sender, EventArgs e)
	{
		var stopwatch = new Stopwatch();

		var platformView = (PlatformView)BindableContainer.Handler!.PlatformView;

		var capturedTimes = new List<long[]>();

		var test1Models = GenerateItems("Test1", 70);
		var test2Models = GenerateItems("Test2", 40);
		var resetModel = GenerateOneItem("Test0");

		for (var i = 0; i < 5; i++)
		{
			await Task.Delay(100);

			Models = test1Models;
			stopwatch.Restart();
			OnPropertyChanged(nameof(Models));
			await WaitForAutomationId(platformView, "Test1");
			stopwatch.Stop();
			var t1 = stopwatch.ElapsedMilliseconds;

			await Task.Delay(100);

			Models = test2Models;
			stopwatch.Restart();
			OnPropertyChanged(nameof(Models));
			await WaitForAutomationId(platformView, "Test2");
			stopwatch.Stop();
			var t2 = stopwatch.ElapsedMilliseconds;

			await Task.Delay(100);

			stopwatch.Restart();
			BindableContainer.Clear();
			Models = resetModel;
			OnPropertyChanged(nameof(Models));
			await WaitForAutomationId(platformView, "Test0");
			stopwatch.Stop();
			var t3 = stopwatch.ElapsedMilliseconds;

			capturedTimes.Add([t1, t2, t3]);
		}

		StartButton.Text =
			$"{capturedTimes.Average(t => t[0])},{capturedTimes.Average(t => t[1])},{capturedTimes.Average(t => t[2])}";
	}

	async Task WaitForAutomationId(PlatformView platformView, string automationId)
	{
		var found = false;

		while (!found)
		{
			await Task.Delay(20);
#if IOS
			int length;
			while ((length = platformView.Subviews.Length) > 0) { platformView = platformView.Subviews[length - 1]; }
			found = platformView.AccessibilityIdentifier == automationId;
#elif ANDROID
			int length;
			while ((length = (platformView as Android.Views.ViewGroup)?.ChildCount ?? 0) > 0) { platformView = ((Android.Views.ViewGroup)platformView!).GetChildAt(length - 1); }
			found = (platformView as Android.Widget.TextView)?.Text == automationId;
#else
			found = true;
#endif
		}
	}

	static List<Model> GenerateItems(string automationId, int count)
	{
		return
		[
			..Enumerable.Range(0, count).Select(i => new Model { Content = $"Content {i}", Header = $"Header {i}" }),
			..GenerateOneItem(automationId)
		];
	}

	static List<Model> GenerateOneItem(string automationId)
	{
		return
		[
			new Model
			{
				Content = automationId,
				Header = automationId,
				SubModels =
				[
					new SubModel { Content = automationId, Header = automationId, AutomationId = automationId }
				]
			}
		];
	}

	public class Model : SubModel
	{
		public SubModel[] SubModels { get; set; } = Enumerable.Range(0, 10).Select(i => new SubModel
		{
			Content = $"SubContent {i}", Header = $"SubHeader {i}"
		}).ToArray();
	}

	public class SubModel
	{
		public string Header { get; set; }
		public string Content { get; set; }
		public string AutomationId { get; set; }
	}
}