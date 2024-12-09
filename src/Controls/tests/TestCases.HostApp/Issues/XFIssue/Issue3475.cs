using System.Diagnostics;

namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 3475, "[iOS] LayoutCompression Performance Issues", PlatformAffected.iOS)]
public class Issue3475 : TestNavigationPage
{
	string _withoutCompressionBtnId = "button1";
	string _withCompressionBtnId = "button2";
	string _titleLabelId = "Label1";

	public static string BackButtonId = "back";
	public static int ItemsCount = 150;
	public static string ElapsedLabelId = "elapsed";
	public static string DoneLabelId = "done";

	protected override void Init()
	{
		var withoutCompressionBtn = new Button
		{
			Text = "Without Layout Compression",
			Command = new Command(async () => await Navigation.PushAsync(new CompressionPage())),
			AutomationId = _withoutCompressionBtnId

		};

		var withCompressionBtn = new Button
		{
			Text = "With Layout Compression",
			Command = new Command(async () => await Navigation.PushAsync(new CompressionPage(true))),
			AutomationId = _withCompressionBtnId
		};

		var content = new StackLayout
		{
			Padding = 10,
			Children =
			{
				new Label
				{
					Text = "Tap buttons to test LayoutCompression Performance in iOS. It should be faster (or at least equal) with LayoutCompression enabled",
					AutomationId = _titleLabelId
				},
				withoutCompressionBtn,
				withCompressionBtn
			}
		};

		Navigation.PushAsync(new ContentPage { Content = content });
	}

	public int GetMs(string text)
	{
		text = text.Replace($"Showing {ItemsCount} items took: ", "", StringComparison.OrdinalIgnoreCase).Replace(" ms", "", StringComparison.OrdinalIgnoreCase);
		return int.TryParse(text, out int elapsed) ? elapsed : 0;
	}
}

public class CompressionPage : ContentPage
{
	readonly Stopwatch _sw = new Stopwatch();
	readonly Label _summaryLabel;
	readonly StackLayout _scrollStack;

	public CompressionPage(bool shouldUseLayoutCompression = false)
	{
		_summaryLabel = new Label { HorizontalOptions = LayoutOptions.Center, BackgroundColor = Colors.Silver, AutomationId = Issue3475.ElapsedLabelId };
		var backButton = new Button { AutomationId = Issue3475.BackButtonId, Text = "Back", Command = new Command(() => Navigation.PopAsync()) };
		_scrollStack = new StackLayout();

		var scrollView = new ScrollView
		{
			Content = _scrollStack
		};

		var mainStack = new StackLayout
		{
			Children =
		{
			_summaryLabel,
			scrollView,
			backButton
		}
		};

		for (int i = 0; i < Issue3475.ItemsCount; i++)
		{
			var childLayout = new StackLayout();

			if (shouldUseLayoutCompression)
			{
				Microsoft.Maui.Controls.CompressedLayout.SetIsHeadless(childLayout, true);
			}

			var label = new Label { Text = $"Item {i}" };
			childLayout.Children.Add(label);
			_scrollStack.Children.Add(childLayout);
		}

		_sw.Start();
		Content = mainStack;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		_sw.Stop();
		_summaryLabel.Text = $"Showing {Issue3475.ItemsCount} items took: {_sw.ElapsedMilliseconds} ms";
		_scrollStack.Children.Insert(0, new Label { Text = "Done", HorizontalOptions = LayoutOptions.Center, AutomationId = Issue3475.DoneLabelId });
	}
}
