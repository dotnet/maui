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
#if ANDROID
			// Clean up any tracker left behind by a scenario run that never completed
			// (the HostApp process is shared across UI tests)
			Issue37012ImeAnimationTracker.Uninstall();
#endif
			return;
		}

		_captureArmed = false;
		StartPaddingCapture();
	}

	// Captures the platform view's padding on the first frame drawn after the page
	// re-attaches, and again once the IME hide animation has completed and the padding has
	// settled. Without the fix the first frame renders with zero safe-area padding (content
	// under the status bar) and the padding only arrives when the IME animation ends.
	async void StartPaddingCapture()
	{
#if ANDROID
		if (Handler?.PlatformView is not Android.Views.View platformView)
		{
			_resultLabel.Text = "Fail: platform view unavailable";
			Issue37012ImeAnimationTracker.Uninstall();
			return;
		}

		// The bug only reproduces when the page re-attaches while the IME hide animation is
		// still in flight; the decor-view tracker installed by Page B makes that observable
		// so a run where the animation already finished reports Inconclusive, not Success.
		bool imeAnimatingAtReattach = Issue37012ImeAnimationTracker.IsImeAnimating;

		var firstDrawTop = -1;
		var firstDrawBottom = -1;
		var preDrawListener = new FirstPreDrawListener(platformView, (top, bottom) =>
		{
			firstDrawTop = top;
			firstDrawBottom = bottom;
		});
		platformView.ViewTreeObserver.AddOnPreDrawListener(preDrawListener);

		try
		{
			// Poll until the IME is fully gone and the padding is stable across consecutive
			// samples instead of sampling once at a fixed delay — slow CI emulators can
			// exceed any hard-coded animation estimate.
			int stableSamples = 0;
			int lastTop = -1, lastBottom = -1;
			for (int i = 0; i < 40 && stableSamples < 3; i++)
			{
				await Task.Delay(100);

				var rootInsets = AndroidX.Core.View.ViewCompat.GetRootWindowInsets(platformView);
				bool imeVisible = rootInsets?.IsVisible(AndroidX.Core.View.WindowInsetsCompat.Type.Ime()) ?? false;
				if (Issue37012ImeAnimationTracker.IsImeAnimating || imeVisible)
				{
					stableSamples = 0;
					continue;
				}

				if (platformView.PaddingTop == lastTop && platformView.PaddingBottom == lastBottom)
				{
					stableSamples++;
				}
				else
				{
					stableSamples = 0;
					lastTop = platformView.PaddingTop;
					lastBottom = platformView.PaddingBottom;
				}
			}

			var finalTop = platformView.PaddingTop;
			var finalBottom = platformView.PaddingBottom;

			// With the keyboard fully hidden, the bottom safe-area padding must equal the
			// non-IME insets — a larger value means keyboard-height padding leaked past the
			// end of the animation (the stale-bottom-padding risk of the gate exemption)
			var settledInsets = AndroidX.Core.View.ViewCompat.GetRootWindowInsets(platformView);
			var expectedBottom = settledInsets?.GetInsets(
				AndroidX.Core.View.WindowInsetsCompat.Type.SystemBars() |
				AndroidX.Core.View.WindowInsetsCompat.Type.DisplayCutout())?.Bottom ?? 0;

			if (!imeAnimatingAtReattach)
			{
				_resultLabel.Text = "Inconclusive: IME animation completed before re-attach";
			}
			else if (finalTop == 0)
			{
				_resultLabel.Text = "Inconclusive: no top inset on this device";
			}
			else if (firstDrawTop != finalTop)
			{
				_resultLabel.Text = $"Fail: first frame top={firstDrawTop} settled top={finalTop}";
			}
			else if (finalBottom != expectedBottom)
			{
				_resultLabel.Text = $"Fail: stale bottom padding {finalBottom}, expected {expectedBottom}";
			}
			else
			{
				_resultLabel.Text = $"Success: top={finalTop} bottom={finalBottom} firstBottom={firstDrawBottom}";
			}
		}
		catch (Exception ex)
		{
			// async void: an unhandled exception would crash the shared HostApp process
			_resultLabel.Text = $"Fail: {ex.Message}";
		}
		finally
		{
			preDrawListener.Detach();
			Issue37012ImeAnimationTracker.Uninstall();
		}
#else
		_resultLabel.Text = "Skipped: not applicable on this platform";
		await Task.CompletedTask;
#endif
	}

#if ANDROID
	sealed class FirstPreDrawListener : Java.Lang.Object, Android.Views.ViewTreeObserver.IOnPreDrawListener
	{
		readonly Android.Views.View _view;
		Action<int, int> _onFirstPreDraw;

		public FirstPreDrawListener(Android.Views.View view, Action<int, int> onFirstPreDraw)
		{
			_view = view;
			_onFirstPreDraw = onFirstPreDraw;
		}

		public bool OnPreDraw()
		{
			_onFirstPreDraw?.Invoke(_view.PaddingTop, _view.PaddingBottom);
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
			// IME hide animation is still in flight. The short delay lets the hide animation
			// start before Page A's platform views re-attach; whether it was still running at
			// re-attach is verified by the tracker, not assumed.
			await entry.HideSoftInputAsync(CancellationToken.None);
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

#if ANDROID
		Loaded += (sender, e) => Issue37012ImeAnimationTracker.Install();
#endif
	}
}

#if ANDROID
// Observes IME animations from the activity decor view (which MAUI does not attach its own
// animation callback to) so the test pages can verify the "re-attached mid-animation"
// precondition. DispatchModeContinueOnSubtree keeps the normal child dispatch intact.
sealed class Issue37012ImeAnimationTracker : AndroidX.Core.View.WindowInsetsAnimationCompat.Callback
{
	static Issue37012ImeAnimationTracker _installed;

	public static bool IsImeAnimating { get; private set; }

	Issue37012ImeAnimationTracker() : base(DispatchModeContinueOnSubtree)
	{
	}

	public static void Install()
	{
		if (_installed is null &&
			Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.Window?.DecorView is Android.Views.View decorView)
		{
			_installed = new Issue37012ImeAnimationTracker();
			AndroidX.Core.View.ViewCompat.SetWindowInsetsAnimationCallback(decorView, _installed);
		}
	}

	// The HostApp process is shared across UI tests; remove the decor callback once the
	// scenario completes so it cannot affect unrelated tests.
	public static void Uninstall()
	{
		if (_installed is not null &&
			Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.Window?.DecorView is Android.Views.View decorView)
		{
			AndroidX.Core.View.ViewCompat.SetWindowInsetsAnimationCallback(decorView, null);
		}

		_installed = null;
		IsImeAnimating = false;
	}

	static bool IsIme(AndroidX.Core.View.WindowInsetsAnimationCompat animation) =>
		(animation.TypeMask & AndroidX.Core.View.WindowInsetsCompat.Type.Ime()) != 0;

	public override void OnPrepare(AndroidX.Core.View.WindowInsetsAnimationCompat animation)
	{
		if (IsIme(animation))
		{
			IsImeAnimating = true;
		}

		base.OnPrepare(animation);
	}

	public override AndroidX.Core.View.WindowInsetsCompat OnProgress(
		AndroidX.Core.View.WindowInsetsCompat insets,
		IList<AndroidX.Core.View.WindowInsetsAnimationCompat> runningAnimations)
	{
		return insets;
	}

	public override void OnEnd(AndroidX.Core.View.WindowInsetsAnimationCompat animation)
	{
		if (IsIme(animation))
		{
			IsImeAnimating = false;
		}

		base.OnEnd(animation);
	}
}
#endif
