using Xamarin.Forms.Platform;

namespace Xamarin.Forms
{
	[ContentProperty("Content")]
	[RenderWith(typeof(_FrameRenderer))]
	public class Frame : ContentView
	{
		public static readonly BindableProperty OutlineColorProperty = BindableProperty.Create("OutlineColor", typeof(Color), typeof(Frame), Color.Default);

		public static readonly BindableProperty HasShadowProperty = BindableProperty.Create("HasShadow", typeof(bool), typeof(Frame), true);

		public Frame()
		{
			Padding = new Size(20, 20);
		}

		public bool HasShadow
		{
			get { return (bool)GetValue(HasShadowProperty); }
			set { SetValue(HasShadowProperty, value); }
		}

		public Color OutlineColor
		{
			get { return (Color)GetValue(OutlineColorProperty); }
			set { SetValue(OutlineColorProperty, value); }
		}
	}
}