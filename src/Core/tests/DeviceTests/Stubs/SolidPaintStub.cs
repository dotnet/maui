using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	partial class SolidPaintStub : SolidPaint
	{
		public SolidPaintStub(Color color)
		{
			Color = color;
		}

#if IOS || __IOS__
		public CoreAnimation.CALayer ToCALayer(CoreGraphics.CGRect frame = default) =>
			new CoreAnimation.CALayer
			{
				ContentsGravity = CoreAnimation.CALayer.GravityResizeAspectFill,
				BackgroundColor = Color.ToCGColor(),
				Frame = frame,
			};
#elif MONOANDROID || __ANDROID__
		public Android.Graphics.Drawables.Drawable ToDrawable(Android.Content.Context context)
		{
			var drawable = new MauiDrawable(context);
			drawable.SetBackgroundColor(Color.ToNative());
			return drawable;
		}
#endif
	}
}