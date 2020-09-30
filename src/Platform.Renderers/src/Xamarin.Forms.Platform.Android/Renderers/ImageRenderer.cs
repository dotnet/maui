using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Xamarin.Forms.Internals;
using AImageView = Android.Widget.ImageView;

namespace Xamarin.Forms.Platform.Android
{
	internal interface IImageRendererController
	{
		void SkipInvalidate();
		bool IsDisposed { get; }
		void SetFormsAnimationDrawable(IFormsAnimationDrawable formsAnimationDrawable);
	}

	public class ImageRenderer : ViewRenderer<Image, AImageView>
	{
		bool _isDisposed;
		readonly MotionEventHelper _motionEventHelper = new MotionEventHelper();

		public ImageRenderer(Context context) : base(context)
		{
			AutoPackage = false;
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use ImageRenderer(Context) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public ImageRenderer()
		{
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

		protected override async void OnElementChanged(ElementChangedEventArgs<Image> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement == null)
			{
				var view = CreateNativeControl();
				SetNativeControl(view);
			}

			_motionEventHelper.UpdateElement(e.NewElement);

			await TryUpdateBitmap(e.OldElement);

			UpdateAspect();

			if (e.NewElement is IImageController imageController && imageController.GetLoadAsAnimation())
				UpdateAnimations();
		}

		protected override async void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (this.IsDisposed())
			{
				return;
			}

			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Image.SourceProperty.PropertyName)
				await TryUpdateBitmap();
			else if (e.PropertyName == Image.AspectProperty.PropertyName)
				UpdateAspect();
			else if (e.Is(Image.IsAnimationPlayingProperty))
				UpdateAnimations();
		}

		void UpdateAnimations()
		{
			Log.Warning(nameof(ImageRenderer), "Animations do not work with Legacy Renderers. Please remove the \"UseLegacyRenderers\" flag or change your renderer to inherit from the fast image renderer.");
		}

		void UpdateAspect()
		{
			if (Element == null || Control == null || Control.IsDisposed())
			{
				return;
			}

			AImageView.ScaleType type = Element.Aspect.ToScaleType();
			Control.SetScaleType(type);
		}

		protected virtual async Task TryUpdateBitmap(Image previous = null)
		{
			// By default we'll just catch and log any exceptions thrown by UpdateBitmap so they don't bring down
			// the application; a custom renderer can override this method and handle exceptions from
			// UpdateBitmap differently if it wants to

			try
			{
				await UpdateBitmap(previous);
			}
			catch (Exception ex)
			{
				Log.Warning(nameof(ImageRenderer), "Error loading image: {0}", ex);
			}
			finally
			{
				((IImageController)Element)?.SetIsLoading(false);
			}
		}

		protected async Task UpdateBitmap(Image previous = null)
		{
			if (Element == null || Control == null || Control.IsDisposed())
			{
				return;
			}

			await Control.UpdateBitmap(Element, previous).ConfigureAwait(false);
		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			if (base.OnTouchEvent(e))
				return true;

			return _motionEventHelper.HandleMotionEvent(Parent, e);
		}
	}
}