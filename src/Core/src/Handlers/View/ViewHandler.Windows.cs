#nullable enable
using NativeView = Microsoft.UI.Xaml.FrameworkElement;

namespace Microsoft.Maui.Handlers
{
	public partial class ViewHandler
	{
		public static void MapTranslationX(ViewHandler handler, IView view) 
		{ 
			((NativeView?)handler.WrappedNativeView)?.UpdateTransformation(view);
		}

		public static void MapTranslationY(ViewHandler handler, IView view) 
		{ 
			((NativeView?)handler.WrappedNativeView)?.UpdateTransformation(view);
		}

		public static void MapScale(ViewHandler handler, IView view) 
		{ 
			((NativeView?)handler.WrappedNativeView)?.UpdateTransformation(view);
		}

		public static void MapScaleX(ViewHandler handler, IView view)
		{
			((NativeView?)handler.WrappedNativeView)?.UpdateTransformation(view);
		}

		public static void MapScaleY(ViewHandler handler, IView view)
		{
			((NativeView?)handler.WrappedNativeView)?.UpdateTransformation(view);
		}

		public static void MapRotation(ViewHandler handler, IView view) 
		{ 
			((NativeView?)handler.WrappedNativeView)?.UpdateTransformation(view); 
		}

		public static void MapRotationX(ViewHandler handler, IView view) 
		{ 
			((NativeView?)handler.WrappedNativeView)?.UpdateTransformation(view);
		}

		public static void MapRotationY(ViewHandler handler, IView view) 
		{ 
			((NativeView?)handler.WrappedNativeView)?.UpdateTransformation(view); 
		}

		public static void MapAnchorX(ViewHandler handler, IView view) 
		{ 
			((NativeView?)handler.WrappedNativeView)?.UpdateTransformation(view); 
		}

		public static void MapAnchorY(ViewHandler handler, IView view) 
		{ 
			((NativeView?)handler.WrappedNativeView)?.UpdateTransformation(view); 
		}
	}
}