using System.Threading;
using Microsoft.Graphics.Canvas;

namespace Microsoft.Maui.Graphics.Win2D
{
	internal class W2DGraphicsService
	{
		private static ICanvasResourceCreator _globalCreator;
		private static readonly ThreadLocal<ICanvasResourceCreator> _threadLocalCreator = new ThreadLocal<ICanvasResourceCreator>();

		public static ICanvasResourceCreator GlobalCreator
		{
			get => _globalCreator;
			set => _globalCreator = value;
		}

		public static ICanvasResourceCreator ThreadLocalCreator
		{
			set => _threadLocalCreator.Value = value;
		}

		public static ICanvasResourceCreator Creator
		{
			get
			{
				var value = _threadLocalCreator.Value;
				if (value == null)
					return _globalCreator;

				return value;
			}

		}
	}
}
