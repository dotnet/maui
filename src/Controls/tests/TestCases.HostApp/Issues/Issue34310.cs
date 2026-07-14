#if ANDROID
using AView = Android.Views.View;
using AViewGroup = Android.Views.ViewGroup;
using AFrameLayout = Android.Widget.FrameLayout;
#elif IOS || MACCATALYST
using UIKit;
#elif WINDOWS
using Microsoft.UI.Xaml;
using WGrid = Microsoft.UI.Xaml.Controls.Grid;
#endif
#nullable enable
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34310, "Loaded event not called for MAUI View added to native View", PlatformAffected.All)]
public class Issue34310 : ContentPage
{
	public Issue34310()
	{
		var statusLabel = new Label
		{
			Text = "Not Loaded",
			AutomationId = "LoadedStatus",
			FontSize = 18,
		};

		var hostedLabel = new Label { Text = "Hosted Content" };
		hostedLabel.Loaded += (s, e) => statusLabel.Text = "Loaded";

		var nativeHostView = new Issue34310NativeHostView
		{
			HostedView = hostedLabel,
			HeightRequest = 80,
			BackgroundColor = Colors.LightBlue,
		};

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 10,
			Children = { statusLabel, nativeHostView }
		};
	}
}

public class Issue34310NativeHostView : View
{
	public static readonly BindableProperty HostedViewProperty =
		BindableProperty.Create(nameof(HostedView), typeof(View), typeof(Issue34310NativeHostView));

	public View? HostedView
	{
		get => (View?)GetValue(HostedViewProperty);
		set => SetValue(HostedViewProperty, value);
	}
}

#if ANDROID
public class Issue34310NativeHostViewHandler : ViewHandler<Issue34310NativeHostView, AFrameLayout>
{
	AView? _nativeChild;
	View? _previousHostedView;

	public static readonly IPropertyMapper<Issue34310NativeHostView, Issue34310NativeHostViewHandler> Mapper =
		new PropertyMapper<Issue34310NativeHostView, Issue34310NativeHostViewHandler>(ViewHandler.ViewMapper)
		{
			[nameof(Issue34310NativeHostView.HostedView)] = MapHostedView,
		};

	public Issue34310NativeHostViewHandler() : base(Mapper) { }

	protected override AFrameLayout CreatePlatformView()
		=> new AFrameLayout(Context)
		{
			LayoutParameters = new AFrameLayout.LayoutParams(
				AViewGroup.LayoutParams.MatchParent,
				AViewGroup.LayoutParams.WrapContent)
		};

	protected override void ConnectHandler(AFrameLayout platformView)
	{
		base.ConnectHandler(platformView);
		UpdateHostedView();
	}

	protected override void DisconnectHandler(AFrameLayout platformView)
	{
		RemoveNativeChild();
		base.DisconnectHandler(platformView);
	}

	static void MapHostedView(Issue34310NativeHostViewHandler handler, Issue34310NativeHostView view)
		=> handler.UpdateHostedView();

	void RemoveNativeChild()
	{
		if (PlatformView is null || _nativeChild is null)
		{
			return;
		}

		PlatformView.RemoveView(_nativeChild);
		_nativeChild = null;
	}

	void UpdateHostedView()
	{
		if (MauiContext is null || PlatformView is null)
		{
			return;
		}

		var hosted = VirtualView?.HostedView;

		// Change detection - only update if the view actually changed
		if (hosted == _previousHostedView)
		{
			return;
		}

		RemoveNativeChild();
		_previousHostedView = hosted;

		if (hosted is null)
		{
			return;
		}

		var native = hosted.ToPlatform(MauiContext) as AView;
		if (native is null)
		{
			return;
		}

		native.LayoutParameters = new AFrameLayout.LayoutParams(
			AViewGroup.LayoutParams.MatchParent,
			AViewGroup.LayoutParams.WrapContent);
		PlatformView.AddView(native);
		_nativeChild = native;
	}
}
#elif IOS || MACCATALYST
public class Issue34310NativeHostViewHandler : ViewHandler<Issue34310NativeHostView, UIView>
{
	UIView? _nativeChild;
	View? _previousHostedView;

	public static readonly IPropertyMapper<Issue34310NativeHostView, Issue34310NativeHostViewHandler> Mapper =
		new PropertyMapper<Issue34310NativeHostView, Issue34310NativeHostViewHandler>(ViewHandler.ViewMapper)
		{
			[nameof(Issue34310NativeHostView.HostedView)] = MapHostedView,
		};

	public Issue34310NativeHostViewHandler() : base(Mapper) { }

	protected override UIView CreatePlatformView()
		=> new UIView { AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight };

	protected override void ConnectHandler(UIView platformView)
	{
		base.ConnectHandler(platformView);
		UpdateHostedView();
	}

	protected override void DisconnectHandler(UIView platformView)
	{
		RemoveNativeChild();
		base.DisconnectHandler(platformView);
	}

	static void MapHostedView(Issue34310NativeHostViewHandler handler, Issue34310NativeHostView view)
		=> handler.UpdateHostedView();

	void RemoveNativeChild()
	{
		if (_nativeChild is null)
			return;

		_nativeChild.RemoveFromSuperview();
		_nativeChild = null;
	}

	void UpdateHostedView()
	{
		if (MauiContext is null || PlatformView is null)
		{
			return;
		}

		var hosted = VirtualView?.HostedView;

		// Change detection - only update if the view actually changed
		if (hosted == _previousHostedView)
		{
			return;
		}

		RemoveNativeChild();
		_previousHostedView = hosted;

		if (hosted is null)
		{
			return;
		}

		var native = hosted.ToPlatform(MauiContext) as UIView;
		if (native is null)
		{
			return;
		}

		native.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
		PlatformView.AddSubview(native);
		_nativeChild = native;
	}
}
#elif WINDOWS
public class Issue34310NativeHostViewHandler : ViewHandler<Issue34310NativeHostView, WGrid>
{
	UIElement? _nativeChild;
	View? _previousHostedView;

	public static readonly IPropertyMapper<Issue34310NativeHostView, Issue34310NativeHostViewHandler> Mapper =
		new PropertyMapper<Issue34310NativeHostView, Issue34310NativeHostViewHandler>(ViewHandler.ViewMapper)
		{
			[nameof(Issue34310NativeHostView.HostedView)] = MapHostedView,
		};

	public Issue34310NativeHostViewHandler() : base(Mapper) { }

	protected override WGrid CreatePlatformView()
		=> new WGrid();

	protected override void ConnectHandler(WGrid platformView)
	{
		base.ConnectHandler(platformView);
		UpdateHostedView();
	}

	protected override void DisconnectHandler(WGrid platformView)
	{
		RemoveNativeChild();
		base.DisconnectHandler(platformView);
	}

	static void MapHostedView(Issue34310NativeHostViewHandler handler, Issue34310NativeHostView view)
		=> handler.UpdateHostedView();

	void RemoveNativeChild()
	{
		if (PlatformView is null || _nativeChild is null)
		{
			return;
		}

		PlatformView.Children.Remove(_nativeChild);
		_nativeChild = null;
	}

	void UpdateHostedView()
	{
		if (MauiContext is null || PlatformView is null)
		{
			return;
		}

		var hosted = VirtualView?.HostedView;

		// Change detection - only update if the view actually changed
		if (hosted == _previousHostedView)
		{
			return;
		}

		RemoveNativeChild();
		_previousHostedView = hosted;

		if (hosted is null)
		{
			return;
		}

		var native = hosted.ToPlatform(MauiContext) as UIElement;
		if (native is null)
		{
			return;
		}

		PlatformView.Children.Add(native);
		_nativeChild = native;
	}
}
#endif