using Android.Content;
using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.Android;
using Xamarin.Forms.Platform.Android.AppCompat;
using Xamarin.Forms.Controls.Issues;
using System.ComponentModel;

[assembly: ExportRenderer(typeof(Issue5724.CustomButton), typeof(CustomButtonRenderer5724))]
namespace Xamarin.Forms.ControlGallery.Android
{
	public class CustomButtonRenderer5724 :
#if TEST_EXPERIMENTAL_RENDERERS
		Platform.Android.FastRenderers.ButtonRenderer
#else
		ButtonRenderer
#endif
	{
		public CustomButtonRenderer5724(Context context) : base(context)
		{
		}

		protected override void OnElementChanged(Platform.Android.ElementChangedEventArgs<Button> e)
		{
			base.OnElementChanged(e);
		}
		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
		}
	}

	public class CustomImageRenderer5724 :
#if TEST_EXPERIMENTAL_RENDERERS
		Platform.Android.FastRenderers.ImageRenderer
#else
		ImageRenderer
#endif
	{
		public CustomImageRenderer5724(Context context) : base(context)
		{
		}

		protected override void OnElementChanged(Platform.Android.ElementChangedEventArgs<Image> e)
		{
			base.OnElementChanged(e);
		}
		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
		}
	}

	public class CustomFrameRenderer5724 :
#if TEST_EXPERIMENTAL_RENDERERS
		Platform.Android.FastRenderers.FrameRenderer
#else
		FrameRenderer
#endif
	{
		public CustomFrameRenderer5724(Context context) : base(context)
		{
		}

		protected override void OnElementChanged(Platform.Android.ElementChangedEventArgs<Frame> e)
		{
			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
		}

	}

	public class CustomLabelRenderer5724 :
#if TEST_EXPERIMENTAL_RENDERERS
		Platform.Android.FastRenderers.LabelRenderer
#else
		LabelRenderer
#endif
	{
		public CustomLabelRenderer5724(Context context) : base(context)
		{
		}

		protected override void OnElementChanged(Platform.Android.ElementChangedEventArgs<Label> e)
		{
			base.OnElementChanged(e);
		}
		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
		}
	}
}