using System.Threading;
using Microsoft.Graphics.Canvas;

#if MAUI_GRAPHICS_WIN2D
namespace Microsoft.Maui.Graphics.Win2D
#else
namespace Microsoft.Maui.Graphics.Platform
#endif
{
	/// <summary>
	/// Some useful APIs for Maui Graphics.
	/// </summary>
#if MAUI_GRAPHICS_WIN2D
	internal class W2DGraphicsService
#else
	internal class PlatformGraphicsService
#endif
	{
		private static ICanvasResourceCreator _globalCreator;
		private static readonly ThreadLocal<ICanvasResourceCreator> _threadLocalCreator = new ThreadLocal<ICanvasResourceCreator>();

		public static ICanvasResourceCreator GlobalCreator
		{
			get => _globalCreator ?? CanvasDevice.GetSharedDevice();
			set => _globalCreator = value;
		}

		public static ICanvasResourceCreator ThreadLocalCreator
		{
			get => _threadLocalCreator.Value;
			set => _threadLocalCreator.Value = value;
		}

		public static ICanvasResourceCreator Creator =>
			ThreadLocalCreator ?? GlobalCreator;
	}
}
