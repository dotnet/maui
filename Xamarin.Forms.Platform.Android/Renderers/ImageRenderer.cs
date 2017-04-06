using System;
using System.ComponentModel;
using Android.Graphics;
using Android.Views;
using AImageView = Android.Widget.ImageView;
using Xamarin.Forms.Internals;
using static Xamarin.Forms.Platform.Android.ImageViewExtensions;

namespace Xamarin.Forms.Platform.Android
{
	internal interface IImageRendererController
	{
		void SkipInvalidate();
	}

	public class ImageRenderer : ViewRenderer<Image, AImageView>
	{
		bool _isDisposed;
		readonly MotionEventHelper _motionEventHelper = new MotionEventHelper();

		public ImageRenderer()
		{
            System.Diagnostics.Debug.WriteLine(">>>>> Old Image Renderer");
			AutoPackage = false;
		}

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			_isDisposed = true;

			base.Dispose(disposing);
		}

		protected override AImageView CreateNativeControl()
		{
			return new FormsImageView(Context);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement == null)
			{
				var view = CreateNativeControl();
				SetNativeControl(view);
			}

			_motionEventHelper.UpdateElement(e.NewElement);
			
			Control.UpdateBitmap(e.NewElement, e.OldElement);

			UpdateAspect();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Image.SourceProperty.PropertyName)
				Control.UpdateBitmap(Element);
			else if (e.PropertyName == Image.AspectProperty.PropertyName)
				UpdateAspect();
		}

		void UpdateAspect()
		{
			AImageView.ScaleType type = Element.Aspect.ToScaleType();
			Control.SetScaleType(type);
		}

        public override bool OnTouchEvent(MotionEvent e)
        {
            if (base.OnTouchEvent(e))
                return true;

            return _motionEventHelper.HandleMotionEvent(Parent);
        }
    }
}