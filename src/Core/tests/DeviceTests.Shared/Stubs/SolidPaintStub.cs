using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class SolidPaintStub : SolidPaint
	{
		public SolidPaintStub(Color color)
		{
			Color = color;
		}

#if IOS || __IOS__ || MACCATALYST
		public CoreAnimation.CALayer ToCALayer(CoreGraphics.CGRect frame = default) =>
			new CoreAnimation.CALayer
			{
				ContentsGravity = CoreAnimation.CALayer.GravityResizeAspectFill,
				BackgroundColor = Color.ToCGColor(),
				Frame = frame,
			};
#elif __ANDROID__
		public global::Android.Graphics.Drawables.Drawable ToDrawable()
		{
			var drawable = new Microsoft.Maui.Graphics.MauiDrawable(MauiProgramDefaults.DefaultContext);
			drawable.SetBackground(new SolidPaint(Color));
			return drawable;
		}
#endif
	}
}