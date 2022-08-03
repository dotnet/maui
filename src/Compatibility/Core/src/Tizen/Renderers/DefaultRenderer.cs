using Microsoft.Maui.Controls.Platform;
using ELayout = ElmSharp.Layout;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	[System.Obsolete]
	public sealed class DefaultRenderer : VisualElementRenderer<VisualElement>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<VisualElement> e)
		{
			if (NativeView == null)
			{
				var control = new ELayout(Forms.NativeParent);
				SetNativeView(control);
			}
			base.OnElementChanged(e);
		}
	}

	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class EllipseRenderer : ShapeRenderer { }

	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class LineRenderer : ShapeRenderer { }

	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class PathRenderer : ShapeRenderer { }

	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class PolygonRenderer : ShapeRenderer { }

	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class PolylineRenderer : ShapeRenderer { }

	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class RectangleRenderer : ShapeRenderer { }

	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class ShapeRenderer : VisualElementRenderer<VisualElement>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<VisualElement> e)
		{
			Log.Info("Use skia render mode (InitializationOptions.UseSkiaSharp=true) to use Shape.");
			if (NativeView == null)
			{
				var control = new ELayout(Forms.NativeParent);
				SetNativeView(control);
			}
			base.OnElementChanged(e);
		}
	}
}
