using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
#if IOS || MACCATALYST
using Microsoft.Maui.Controls.Handlers.Items2;
using UIKit;
#endif

namespace MauiApp3.Pages;

// Manual diagnostics for the two review concerns on dotnet/maui#39778.
public sealed class CollectionViewMeasurementConcernsPage : ContentPage
{
	readonly VerticalStackLayout _results = new() { Spacing = 12 };
#if IOS || MACCATALYST
	readonly List<VisualElement> _mounted = new();
#endif

	public CollectionViewMeasurementConcernsPage()
	{
		Title = "CV measurement concerns";
		var run = new Button
		{
			Text = "Run fresh pre-mount cases, then mount vertical lists",
			AutomationId = "Run39778Measurements"
		};
#if IOS || MACCATALYST
		run.Clicked += (_, _) => RunCases();
#else
		run.IsEnabled = false;
		_results.Children.Add(new Label { Text = "Requires the iOS/MacCatalyst Items2 handler." });
#endif
		Content = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				Padding = 16,
				Spacing = 12,
				Children =
				{
					new Label
					{
						Text = "#39778: manual diagnostics, not pass/fail assertions. Uses local source CollectionViewHandler2 instances; " +
							"no app-wide registration changes. Run after opening this page. Each case starts with a fresh, unattached native list."
					},
					new Label
					{
						Text = "1. Vertical: WidthRequest=250, three 50-high rows, NO HeightRequest or minimum height. " +
							"A normal HorizontalStackLayout supplies ∞ width; a ContentView supplies finite width as control. " +
							"Both parents are first measured at 350 × 400. Expected intrinsic height: 150, not a forced presentation height. " +
							"After measurement they mount in 350 × 400 viewports, aligned at the top; inspect actual list height and visible rows."
					},
					new Label
					{
						Text = "2. Horizontal: MinimumHeightRequest=250, three labels WidthRequest=200, no explicit list height. " +
							"Normal IView.Measure(350, 400) is called twice before mounting; a separate fresh list uses (350, ∞). " +
							"400 is only a candidate native-probe height requiring empirical confirmation; 250 is the fallback candidate. " +
							"A value in 250..400 alone does NOT prove native probing occurred. Inspect raw calls, native content sizes and frame restoration."
					},
					run,
					_results
				}
			}
		};
	}

#if IOS || MACCATALYST
	void RunCases()
	{
		_results.Children.Clear();
		foreach (var view in _mounted)
		{
			view.Handler?.DisconnectHandler();
			view.Handler = null;
		}
		_mounted.Clear();
		try
		{
			AddVerticalCase(horizontalParent: true);
			AddVerticalCase(horizontalParent: false);
			AddHorizontalCase(400, "FiniteHeight");
			AddHorizontalCase(double.PositiveInfinity, "InfiniteHeight");
		}
		catch (Exception exception)
		{
			_results.Children.Add(new Label { Text = $"Diagnostics stopped: {exception}" });
		}
	}

	static CollectionView CreateList(bool horizontal) => new()
	{
		ItemsLayout = new LinearItemsLayout(horizontal ? ItemsLayoutOrientation.Horizontal : ItemsLayoutOrientation.Vertical)
		{
			ItemSpacing = 0
		},
		ItemsSource = new[] { "Row 1", "Row 2", "Row 3" },
		SelectionMode = SelectionMode.None,
		BackgroundColor = Colors.LightBlue,
		ItemTemplate = new DataTemplate(() =>
		{
			var label = new Label
			{
				TextColor = Colors.Black,
				BackgroundColor = Colors.LightYellow,
				LineBreakMode = LineBreakMode.NoWrap,
				Margin = 0
			};
			if (horizontal)
				label.WidthRequest = 200;
			else
				label.HeightRequest = 50;
			label.SetBinding(Label.TextProperty, ".");
			return label;
		})
	};

	RecordingHandler Attach(CollectionView list, bool horizontal)
	{
		var context = Handler?.MauiContext ?? throw new InvalidOperationException("Open the page before running.");
		var handler = new RecordingHandler(horizontal);
		handler.SetMauiContext(context);
		list.Handler = handler;
		return handler;
	}

	void AddVerticalCase(bool horizontalParent)
	{
		var name = horizontalParent ? "HorizontalStack" : "FiniteWidthControl";
		var list = CreateList(horizontal: false);
		list.AutomationId = "CV39778-" + name;
		list.WidthRequest = 250;
		var handler = Attach(list, horizontal: false);
		View parent;
		if (horizontalParent)
			parent = new HorizontalStackLayout { Children = { list }, Spacing = 0 };
		else
			parent = new ContentView { Content = list };
		_mounted.Add(list);
		_mounted.Add(parent);
		// Native children exist, but no parent has been put in the page/window yet.
		Microsoft.Maui.Platform.ElementExtensions.ToHandler(parent, handler.MauiContext!);
		var native = Native(handler);
		if (native.Window != null)
			throw new InvalidOperationException("Vertical case was already mounted.");
		var before = State(native);
		// Exercise the real parent's measure path, not a manually injected infinite constraint.
		var first = ((IView)parent).Measure(350, 400);
		var firstChild = ((IView)list).DesiredSize;
		var repeated = ((IView)parent).Measure(350, 400);
		var repeatedChild = ((IView)list).DesiredSize;
		handler.Capture = false;
		AddReport(name + "Measured",
			$"1 / {name}: pre-mount normal parent.Measure(350 × 400)\n" +
			$"Before: {before}\n" +
			$"First parent={Dimensions(first)}, child={Dimensions(firstChild)}\n" +
			$"Repeated parent={Dimensions(repeated)}, child={Dimensions(repeatedChild)}\n" +
			$"{handler.Report}\nExpected child intrinsic height=150; observed first delta={Number(firstChild.Height - 150)}");

		var mounted = new Label { Text = "Awaiting mount/layout.", AutomationId = name + "Mounted" };
		// Only the viewport has fixed height. Do not mask intrinsic size with a list HeightRequest.
		parent.VerticalOptions = LayoutOptions.Start;
		var viewport = new Grid
		{
			WidthRequest = 350,
			HeightRequest = 400,
			HorizontalOptions = LayoutOptions.Start,
			BackgroundColor = Colors.LightGray,
			Children = { parent }
		};
		_mounted.Add(viewport);
		void ReportMounted()
		{
			if (list.Handler != handler || native.Window == null)
				return;
			mounted.Text = $"{name} POST-MOUNT: viewport={Dimensions(viewport.Width, viewport.Height)}, " +
				$"parent={Dimensions(parent.Width, parent.Height)}, list={Dimensions(list.Width, list.Height)}\n" +
				$"List desired={Dimensions(((IView)list).DesiredSize)}, height delta from 150={Number(list.Height - 150)}\n" +
				$"{State(native)}";
		}
		void QueueReport() => Dispatcher.Dispatch(ReportMounted);
		list.SizeChanged += (_, _) => QueueReport();
		viewport.SizeChanged += (_, _) => QueueReport();
		list.Loaded += (_, _) => QueueReport();
		var refresh = new Button { Text = $"Read {name} mounted state", AutomationId = name + "Refresh" };
		refresh.Clicked += (_, _) => ReportMounted();
		_results.Children.Add(mounted);
		_results.Children.Add(refresh);
		_results.Children.Add(viewport);
	}

	void AddHorizontalCase(double height, string name)
	{
		var list = CreateList(horizontal: true);
		list.AutomationId = "CV39778-" + name;
		list.MinimumHeightRequest = 250;
		var handler = Attach(list, horizontal: true);
		try
		{
			var native = Native(handler);
			if (native.Window != null)
				throw new InvalidOperationException("Horizontal case was already mounted.");
			var before = State(native);
			// Two normal calls on the SAME instance expose cache/repeat behavior; each
			// constraint case has its OWN fresh instance, never primed by the other case.
			var first = ((IView)list).Measure(350, height);
			var repeated = ((IView)list).Measure(350, height);
			handler.Capture = false;
			AddReport(name + "Measured",
				$"2 / {name}: pre-mount IView.Measure({Dimensions(350, height)}) twice\n" +
				$"Before: {before}\nFirst={Dimensions(first)}; repeated={Dimensions(repeated)}\n" +
				$"{handler.Report}\n" +
				"Interpret with native evidence: 400=candidate finite probe, 250=candidate minimum/fallback; neither is an automatic pass.");
		}
		finally
		{
			((IElementHandler)handler).DisconnectHandler();
			list.Handler = null;
		}
	}

	void AddReport(string id, string text)
	{
		_results.Children.Add(new Label { Text = text, AutomationId = id });
	}

	static string Number(double value) => double.IsPositiveInfinity(value) ? "∞" : value.ToString("0.##");
	static string Dimensions(double width, double height) => $"{Number(width)} × {Number(height)}";
	static string Dimensions(Size size) => Dimensions(size.Width, size.Height);

	static UICollectionView Native(RecordingHandler handler) =>
		FindCollectionView(handler.PlatformView) ?? throw new InvalidOperationException("No native UICollectionView found.");

	static UICollectionView? FindCollectionView(UIView view)
	{
		if (view is UICollectionView collectionView)
			return collectionView;
		foreach (var child in view.Subviews)
			if (FindCollectionView(child) is { } result)
				return result;
		return null;
	}

	static string State(UICollectionView native)
	{
		var layout = native.CollectionViewLayout.CollectionViewContentSize;
		return $"window={(native.Window == null ? "null" : "attached")}, " +
			$"frame=({Number(native.Frame.X)}, {Number(native.Frame.Y)}, {Dimensions(native.Frame.Width, native.Frame.Height)}), " +
			$"layout={Dimensions(layout.Width, layout.Height)}, content={Dimensions(native.ContentSize.Width, native.ContentSize.Height)}";
	}

	// Observe public GetDesiredSize DURING normal measurement; do not make a second
	// diagnostic call that would prime native layout before the real measurement.
	sealed class RecordingHandler(bool horizontal) : CollectionViewHandler2
	{
		readonly List<string> _calls = new();
		public bool Capture { get; set; } = true;
		public string Report => $"Handler={GetType().BaseType?.FullName}\nAssembly={GetType().BaseType?.Assembly.FullName}\n" +
			(_calls.Count == 0 ? "No handler calls observed (normal measure may have been cached)." : string.Join("\n", _calls));

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (!Capture)
				return base.GetDesiredSize(widthConstraint, heightConstraint);
			var native = Native(this);
			var frame = native.Frame;
			var before = State(native);
			var layout = native.CollectionViewLayout.CollectionViewContentSize;
			var incomingCrossAxisFinite = double.IsFinite(horizontal ? heightConstraint : widthConstraint);
			var scrollSizeZero = horizontal ? layout.Width == 0 : layout.Height == 0;
			var result = base.GetDesiredSize(widthConstraint, heightConstraint);
			_calls.Add($"Raw call {_calls.Count + 1}: input={Dimensions(widthConstraint, heightConstraint)}, returned={Dimensions(result)}; " +
				$"incoming cross-axis finite={incomingCrossAxisFinite}, initial scroll content zero={scrollSizeZero}\n" +
				$"  Before: {before}\n  After: {State(native)}; frame restored={frame.Equals(native.Frame)}");
			return result;
		}
	}
#endif
}
