using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	partial class BrushStub : IBrush
	{
		readonly Color _color;

		public BrushStub(Color color)
		{
			_color = color;
		}

		public bool IsEmpty => false;

#if IOS || __IOS__
		public CoreAnimation.CALayer ToCALayer(CoreGraphics.CGRect frame = default) =>
			new CoreAnimation.CALayer
			{
				ContentsGravity = CoreAnimation.CALayer.GravityResizeAspectFill,
				BackgroundColor = _color.ToCGColor(),
				Frame = frame,
			};
#elif MONOANDROID || __ANDROID__
		public Android.Graphics.Drawables.Drawable ToDrawable()
		{
			var drawable = new Microsoft.Maui.Graphics.MauiDrawable();
			drawable.SetColor(_color.ToNative());
			return drawable;
		}
#endif
	}
}