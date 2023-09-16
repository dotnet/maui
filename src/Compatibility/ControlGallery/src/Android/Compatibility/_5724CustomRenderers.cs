using System;
using System.ComponentModel;
using System.Linq;
using Android.Content;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.ControlGallery.Issues;
using Microsoft.Maui.Controls.Compatibility.Platform.Android.AppCompat;
using Microsoft.Maui.Controls.ControlGallery.Android;
using Microsoft.Maui.Controls.Platform;

[assembly: ExportRenderer(typeof(Issue5724.CustomButton), typeof(CustomButtonRenderer5724))]
namespace Microsoft.Maui.Controls.ControlGallery.Android
{
	public class CustomButtonRenderer5724 :
#if !LEGACY_RENDERERS
		Compatibility.Platform.Android.FastRenderers.ButtonRenderer
#else
		ButtonRenderer
#endif
	{
		public CustomButtonRenderer5724(Context context) : base(context)
		{
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
		{
			base.OnElementChanged(e);
		}
		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
		}
	}

	public class CustomImageRenderer5724 :
#if !LEGACY_RENDERERS
		Compatibility.Platform.Android.FastRenderers.ImageRenderer
#else
		ImageRenderer
#endif
	{
		public CustomImageRenderer5724(Context context) : base(context)
		{
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
		{
			base.OnElementChanged(e);
		}
		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
		}
	}

	public class CustomFrameRenderer5724 : Handlers.Compatibility.FrameRenderer
	{
		public CustomFrameRenderer5724(Context context) : base(context)
		{
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Frame> e)
		{
			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
		}

	}

	public class CustomLabelRenderer5724 :
#if !LEGACY_RENDERERS
		Compatibility.Platform.Android.FastRenderers.LabelRenderer
#else
		LabelRenderer
#endif
	{
		public CustomLabelRenderer5724(Context context) : base(context)
		{
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
		{
			base.OnElementChanged(e);
		}
		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
		}
	}
}