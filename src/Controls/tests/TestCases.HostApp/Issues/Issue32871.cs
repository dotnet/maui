#if ANDROID
using Android.Views;
using AView = Android.Views.View;
#endif

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32871, "[Android] Bottom insets issues when keyboard is shown", PlatformAffected.Android)]
public partial class Issue32871 : ContentPage
{
	public Issue32871()
	{
		SafeAreaEdges = SafeAreaEdges.None;
		BackgroundColor = Colors.Green;

		var paddingLabel = new Label
		{
			Text = "waiting",
			AutomationId = "PaddingLabel",
			TextColor = Colors.White,
			FontSize = 12
		};

		var entry = new Entry
		{
			Placeholder = "Tap here to show keyboard",
			AutomationId = "TestEntry",
			VerticalOptions = LayoutOptions.Start,
			HorizontalOptions = LayoutOptions.Fill,
			HeightRequest = 56
		};

		var grid = new Grid
		{
			AutomationId = "MainGrid",
			SafeAreaEdges = SafeAreaEdges.Default,
			BackgroundColor = Colors.Red,
			RowDefinitions =
			{
				new RowDefinition(80),
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star),
				new RowDefinition(GridLength.Auto)
			}
		};

		var label = new Label
		{
			Text = "Issue 32871",
			AutomationId = "HeaderLabel",
			HorizontalTextAlignment = Microsoft.Maui.TextAlignment.Center,
			VerticalOptions = LayoutOptions.Start,
			TextColor = Colors.White
		};

		var bottomButton = new Button
		{
			Text = "Bottom Button",
			AutomationId = "BottomButton",
			BackgroundColor = Colors.Blue,
			TextColor = Colors.White
		};

		Grid.SetRow(label, 0);
		Grid.SetRow(paddingLabel, 1);
		Grid.SetRow(entry, 2);
		Grid.SetRow(bottomButton, 3);

		grid.Children.Add(label);
		grid.Children.Add(paddingLabel);
		grid.Children.Add(entry);
		grid.Children.Add(bottomButton);

		Content = grid;

		SetupPlatform(grid, paddingLabel);
	}

	partial void SetupPlatform(Grid grid, Label paddingLabel);

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		CleanupPlatform();
	}

	partial void CleanupPlatform();
}

#if ANDROID
public partial class Issue32871
{
	SoftInput _previousSoftInputMode;

	partial void SetupPlatform(Grid grid, Label paddingLabel)
	{
		var window = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.Window;
		if (window?.Attributes is WindowManagerLayoutParams attr)
		{
			_previousSoftInputMode = attr.SoftInputMode;
		}
		window?.SetSoftInputMode(SoftInput.AdjustUnspecified | SoftInput.StateHidden);

		grid.HandlerChanged += (s, e) =>
		{
			if (grid.Handler?.PlatformView is AView nativeView)
			{
				paddingLabel.Text = $"NativePadding: B={nativeView.PaddingBottom}";
				nativeView.AddOnLayoutChangeListener(new LayoutListener(nativeView, paddingLabel));
			}
		};
	}

	partial void CleanupPlatform()
	{
		var window = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.Window;
		window?.SetSoftInputMode(_previousSoftInputMode);
	}

	class LayoutListener : Java.Lang.Object, AView.IOnLayoutChangeListener
	{
		readonly WeakReference<AView> _view;
		readonly WeakReference<Label> _label;

		public LayoutListener(AView view, Label label)
		{
			_view = new WeakReference<AView>(view);
			_label = new WeakReference<Label>(label);
		}

		public void OnLayoutChange(AView v, int left, int top, int right, int bottom,
			int oldLeft, int oldTop, int oldRight, int oldBottom)
		{
			if (_view.TryGetTarget(out var view) && _label.TryGetTarget(out var label))
			{
				label.Text = $"NativePadding: B={view.PaddingBottom}";
			}
		}
	}
}
#endif
