using System;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;

#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIView;
using LayoutPlatformView = Microsoft.Maui.Platform.LayoutView;
#elif __ANDROID__
using PlatformView = Android.Views.View;
using LayoutPlatformView = Microsoft.Maui.Platform.LayoutViewGroup;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
using LayoutPlatformView = Microsoft.Maui.Platform.LayoutPanel;
#elif TIZEN
using PlatformView = Tizen.NUI.BaseComponents.View;
using LayoutPlatformView = Microsoft.Maui.Platform.LayoutViewGroup;
#elif (NETSTANDARD || !PLATFORM)
using PlatformView = System.Object;
using LayoutPlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers;

internal partial class HeadlessLayoutHandler : IHeadlessLayoutHandler
{
	public static CommandMapper<ILayout, IHeadlessLayoutHandler> CommandMapper = new()
	{
		[nameof(ILayoutHandler.Add)] = MapAdd,
		[nameof(ILayoutHandler.Remove)] = MapRemove,
		[nameof(ILayoutHandler.Clear)] = MapClear,
		[nameof(ILayoutHandler.Insert)] = MapInsert,
		[nameof(ILayoutHandler.Update)] = MapUpdate,
		[nameof(ILayoutHandler.UpdateZIndex)] = MapUpdateZIndex,
		[nameof(IView.ZIndex)] = MapZIndex,
	};

	public static void MapAdd(IHeadlessLayoutHandler handler, ILayout layout, object? arg)
	{
		if (arg is LayoutHandlerUpdate args)
		{
			handler.Add(args.View);
		}
	}

	public static void MapRemove(IHeadlessLayoutHandler handler, ILayout layout, object? arg)
	{
		if (arg is LayoutHandlerUpdate args)
		{
			handler.Remove(args.View);
		}
	}

	public static void MapInsert(IHeadlessLayoutHandler handler, ILayout layout, object? arg)
	{
		if (arg is LayoutHandlerUpdate args)
		{
			handler.Insert(args.Index, args.View);
		}
	}

	public static void MapClear(IHeadlessLayoutHandler handler, ILayout layout, object? arg)
	{
		handler.Clear();
	}

	static void MapUpdate(IHeadlessLayoutHandler handler, ILayout layout, object? arg)
	{
		if (arg is LayoutHandlerUpdate args)
		{
			handler.Update(args.Index, args.View);
		}
	}

	static void MapUpdateZIndex(IHeadlessLayoutHandler handler, ILayout layout, object? arg)
	{
		if (arg is IView view)
		{
			handler.UpdateZIndex(view);
		}
	}
	
	static void MapZIndex(ILayoutHandler handler, ILayout view, object? arg)
	{
		if (view.Parent is ILayout layout)
		{
			(layout.Handler as ILayoutHandler)?.UpdateZIndex(view);
		}
	}

	ILayout? _virtualView;
	ILayoutHandler? _platformHandler;
	ILayoutHandler PlatformHandler => _platformHandler ?? throw new InvalidOperationException($"{nameof(PlatformHandler)} should have been set previously.");
	IView? IViewHandler.VirtualView => _virtualView;
	IElement? IElementHandler.VirtualView => _virtualView;
	object IElementHandler.PlatformView => PlatformHandler.PlatformView;

	public ILayout VirtualView
	{
		get => _virtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set.");
	}
	
	public LayoutPlatformView PlatformView => PlatformHandler.PlatformView;
	
	public IMauiContext? MauiContext { get; private set; }
	
	public bool HasContainer
	{
		get => false;
		set
		{
			if (value)
			{
				throw new InvalidOperationException("CompressedLayout does not support HasContainer");
			}
		}
	}

	public object? ContainerView => null;
	
	public void SetMauiContext(IMauiContext mauiContext)
	{
		MauiContext = mauiContext;
	}

	public void SetVirtualView(IElement view)
	{
		_ = view ?? throw new ArgumentNullException(nameof(view));
		_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set.");

		if (ReferenceEquals(_virtualView, view))
		{
			return;
		}

		var oldVirtualView = _virtualView;

		_virtualView = view as ILayout ?? throw new InvalidOperationException($"{nameof(view)} must be an {nameof(ILayout)}.");
		_platformHandler = FindPlatformHandler(_virtualView) ?? throw new InvalidOperationException($"No ancestor platform {nameof(ILayoutHandler)} found for {nameof(ICompressedLayout)}.");

		// We set the previous virtual view to null after setting it on the incoming virtual view.
		// This makes it easier for the incoming virtual view to have influence
		// on how the exchange of handlers happens.
		// We will just set the handler to null ourselves as a last resort cleanup
		if (oldVirtualView?.Handler != null)
		{
			oldVirtualView.Handler = null;
		}
	}

	public void UpdateValue(string property)
	{
		
	}

	public void Invoke(string command, object? args = null)
	{
		if (VirtualView is { } virtualView)
		{
			CommandMapper.Invoke(this, virtualView, command, args);
		}
	}

	public void DisconnectHandler()
	{
		
	}

	public void UpdateZIndex(IView view)
	{
		PlatformHandler.UpdateZIndex(view);
	}

	public Size GetDesiredSize(double widthConstraint, double heightConstraint)
	{
		var desiredSize = VirtualView.CrossPlatformMeasure(widthConstraint, heightConstraint);
		return desiredSize;
	}

	public void PlatformArrange(Rect frame)
	{
		VirtualView.CrossPlatformArrange(frame);
	}
	
	static ILayoutHandler? FindPlatformHandler(ILayout? layout)
	{
		while (layout is { Handler: HeadlessLayoutHandler })
		{
			layout = layout.Parent as ILayout;
		}
		
		return layout?.Handler as ILayoutHandler;
	}

	static IEnumerable<PlatformView> CollectPlatformViews(ILayout layout)
	{
		foreach (var view in layout)
		{
			var platformViewHandler = view.Handler ?? throw new InvalidOperationException($"{nameof(view.Handler)} should have been previously set.");
			if (platformViewHandler is IHeadlessLayoutHandler headlessLayoutHandler)
			{
				foreach (var platformView in CollectPlatformViews(headlessLayoutHandler.VirtualView))
				{
					yield return platformView;
				}
				
				continue;
			}
			
			yield return platformViewHandler.ToPlatform();
		}
	}
}