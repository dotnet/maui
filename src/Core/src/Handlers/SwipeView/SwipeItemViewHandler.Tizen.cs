using System;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Handlers
{
	public class SwipeItemViewHandler : ViewHandler<ISwipeItemView, ContentCanvas>
	{
		INativeViewHandler? _contentHandler;

		public static IPropertyMapper<ISwipeItemView, SwipeItemViewHandler> Mapper = new PropertyMapper<ISwipeItemView, SwipeItemViewHandler>(ViewHandler.ViewMapper)
		{
			[nameof(ISwipeItemView.Content)] = MapContent,
			[nameof(ISwipeItemView.Visibility)] = MapVisibility
		};

		public static CommandMapper<ISwipeItemView, ISwipeViewHandler> CommandMapper = new(ViewHandler.ViewCommandMapper)
		{
		};

		public SwipeItemViewHandler() : base(Mapper, CommandMapper)
		{

		}

		protected SwipeItemViewHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null)
			: base(mapper, commandMapper ?? CommandMapper)
		{
		}

		public SwipeItemViewHandler(IPropertyMapper? mapper = null) : base(mapper ?? Mapper)
		{

		}

		protected override ContentCanvas CreateNativeView()
		{
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a Page");
			_ = NativeParent ?? throw new InvalidOperationException($"{nameof(NativeParent)} cannot be null");

			var view = new ContentCanvas(NativeParent, VirtualView)
			{
				CrossPlatformMeasure = VirtualView.CrossPlatformMeasure,
				CrossPlatformArrange = VirtualView.CrossPlatformArrange
			};

			view.Show();
			return view;
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");

			NativeView.CrossPlatformMeasure = VirtualView.CrossPlatformMeasure;
			NativeView.CrossPlatformArrange = VirtualView.CrossPlatformArrange;
		}

		void UpdateContent()
		{
			_ = NativeView ?? throw new InvalidOperationException($"{nameof(NativeView)} should have been set by base class.");
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			// Cleanup the old view when reused
			NativeView.Children.Clear();
			_contentHandler?.Dispose();
			_contentHandler = null;

			if (VirtualView.PresentedContent is IView view)
			{
				NativeView.Children.Add(view.ToNative(MauiContext));
				if (view.Handler is INativeViewHandler thandler)
				{
					thandler?.SetParent(this);
					_contentHandler = thandler;
				}
			}
		}

		public static void MapContent(SwipeItemViewHandler handler, ISwipeItemView page)
		{
			handler.UpdateContent();
		}

		public static void MapVisibility(SwipeItemViewHandler handler, ISwipeItemView view)
		{
			//TODO : need to update
			//var swipeView = handler.NativeView.GetParentOfType<EvasObject>();
			//if (swipeView != null)
			//	swipeView.UpdateIsVisibleSwipeItem(view);

			//handler.NativeView.UpdateVisibility(view.Visibility);
		}
	}
}
