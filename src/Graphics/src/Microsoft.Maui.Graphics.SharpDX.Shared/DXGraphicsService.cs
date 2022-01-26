using System.IO;
using System.Threading;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.WIC;
using Factory = SharpDX.Direct2D1.Factory;
using FactoryType = SharpDX.Direct2D1.FactoryType;

namespace Microsoft.Maui.Graphics.SharpDX
{
	internal class DXGraphicsService
	{
		public static Factory SharedFactory = new Factory(FactoryType.MultiThreaded);

		public static readonly ThreadLocal<Factory> CurrentFactory = new ThreadLocal<Factory>();
		public static readonly ThreadLocal<RenderTarget> CurrentTarget = new ThreadLocal<RenderTarget>();
		public static ImagingFactory2 FactoryImaging = new ImagingFactory2();

		public static global::SharpDX.DirectWrite.Factory FactoryDirectWrite = new global::SharpDX.DirectWrite.Factory(global::SharpDX.DirectWrite.FactoryType.Shared);

		//public RectangleF GetPathBounds(PathF path)
		//      {
		//          if (path.NativePath is PathGeometry nativePath)
		//          {
		//              return nativePath.GetBounds().AsEWRectangle();
		//          }

		//          if (CurrentFactory.Value != null && path.Closed)
		//          {
		//              if (CurrentFactory.Value != SharedFactory)
		//              {
		//                  nativePath = path.AsDxPath(CurrentFactory.Value);
		//                  if (nativePath != null)
		//                  {
		//                      path.NativePath = nativePath;
		//                      return nativePath.GetBounds().AsEWRectangle();
		//                  }
		//              }
		//          }

		//          var bounds = path.GetBoundsByFlattening();
		//          return bounds;
		//      }
	}
}
