namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 37012, "Safe-area padding stale for the whole IME hide animation when swapping pages", PlatformAffected.Android)]
public class Issue37012 : NavigationPage
{
	public Issue37012() : base(new Issue37012PageA())
	{
	}
}

public class Issue37012PageA : ContentPage
{
	readonly Label _resultLabel;
	bool _captureArmed;

	public Issue37012PageA()
	{
		NavigationPage.SetHasNavigationBar(this, false);
		SafeAreaEdges = SafeAreaEdges.All;

		_resultLabel = new Label
		{
			Text = "Waiting",
			AutomationId = "ResultLabel"
		};

		var openPageBButton = new Button
		{
			Text = "Open Page B",
			AutomationId = "OpenPageB"
		};

		openPageBButton.Clicked += async (sender, e) =>
		{
			_resultLabel.Text = "Waiting";
			_captureArmed = true;
			await Navigation.PushAsync(new Issue37012PageB(), animated: false);
		};

		Content = new VerticalStackLayout
		{
			Spacing = 12,
			Children =
			{
				new Label { Text = "Issue 37012 - Page A" },
				openPageBButton,
				_resultLabel
			}
		};

		Loaded += OnPageLoaded;
	}

	void OnPageLoaded(object sender, EventArgs e)
	{
		if (!_captureArmed)
		{
			return;
		}

		_captureArmed = false;
		StartPaddingCapture();
	}

	// Captures the platform view's top padding on the first frame drawn after the page
	// re-attaches, and again once the IME hide animation has certainly completed. Without
	// the fix the first frame renders with zero safe-area padding (content under the status
	// bar) and the padding only arrives when the IME animation ends.
	void StartPaddingCapture()
	{
#if ANDROID
		if (Handler?.PlatformView is not Android.Views.View platformView)
		{
			_resultLabel.Text = "Fail: platform view unavailable";
			return;
		}

		var firstDrawPadding = -1;
		var preDrawListener = new FirstPreDrawListener(platformView, padding => firstDrawPadding = padding);
		platformView.ViewTreeObserver.AddOnPreDrawListener(preDrawListener);

		Dispatcher.StartTimer(TimeSpan.FromMilliseconds(800), () =>
		{
			preDrawListener.Detach();
			var finalPadding = platformView.PaddingTop;

			if (finalPadding > 0 && firstDrawPadding == finalPadding)
			{
				_resultLabel.Text = $"Success: first={firstDrawPadding} final={finalPadding}";
			}
			else
			{
				_resultLabel.Text = $"Fail: first={firstDrawPadding} final={finalPadding}";
			}

			return false;
		});
#else
		_resultLabel.Text = "Success: not applicable on this platform";
#endif
	}

#if ANDROID
	sealed class FirstPreDrawListener : Java.Lang.Object, Android.Views.ViewTreeObserver.IOnPreDrawListener
	{
		readonly Android.Views.View _view;
		Action<int> _onFirstPreDraw;

		public FirstPreDrawListener(Android.Views.View view, Action<int> onFirstPreDraw)
		{
			_view = view;
			_onFirstPreDraw = onFirstPreDraw;
		}

		public bool OnPreDraw()
		{
			_onFirstPreDraw?.Invoke(_view.PaddingTop);
			Detach();
			return true;
		}

		public void Detach()
		{
			if (_onFirstPreDraw is null)
			{
				return;
			}

			_onFirstPreDraw = null;
			if (_view.ViewTreeObserver?.IsAlive == true)
			{
				_view.ViewTreeObserver.RemoveOnPreDrawListener(this);
			}
		}
	}
#endif
}

public class Issue37012PageB : ContentPage
{
	public Issue37012PageB()
	{
		NavigationPage.SetHasNavigationBar(this, false);
		SafeAreaEdges = SafeAreaEdges.All;

		var entry = new Entry
		{
			AutomationId = "PageBEntry",
			Placeholder = "Tap to open keyboard"
		};

		var hideAndPopButton = new Button
		{
			Text = "Hide keyboard and pop",
			AutomationId = "HideAndPopButton"
		};

		hideAndPopButton.Clicked += async (sender, e) =>
		{
			// Mirror the issue repro: request the keyboard hide, then swap pages while the
			// IME hide animation is still in flight. The short delay guarantees the hide
			// animation has started before Page A's platform views re-attach.
			_ = entry.HideSoftInputAsync(CancellationToken.None);
			await Task.Delay(50);
			await Navigation.PopAsync(animated: false);
		};

		Content = new VerticalStackLayout
		{
			Spacing = 12,
			Children =
			{
				new Label { Text = "Issue 37012 - Page B" },
				entry,
				hideAndPopButton
			}
		};
	}
}
