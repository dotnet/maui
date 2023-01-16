using Microsoft.Graphics.Canvas;
using System.Threading;

namespace Microsoft.Maui.Graphics.Win2D
{
	internal class W2DGraphicsService
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
