using ESize = ElmSharp.Size;
using Specific = Microsoft.Maui.Controls.Compatibility.PlatformConfiguration.TizenSpecific.Image;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen.SkiaSharp
{
	public class ImageRenderer : CanvasViewRenderer<Image, Native.Image>
	{
		public ImageRenderer()
		{
			RegisterPropertyHandler(Image.SourceProperty, UpdateSource);
			RegisterPropertyHandler(Image.AspectProperty, UpdateAspect);
			RegisterPropertyHandler(Image.IsOpaqueProperty, UpdateIsOpaque);
			RegisterPropertyHandler(Image.IsAnimationPlayingProperty, UpdateIsAnimationPlaying);
			RegisterPropertyHandler(Specific.BlendColorProperty, UpdateBlendColor);
			RegisterPropertyHandler(Specific.FileProperty, UpdateFile);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
		{
			if (RealControl == null)
			{
				SetRealNativeControl(new Native.Image(Forms.NativeParent));
			}
			base.OnElementChanged(e);
		}

		protected override ESize Measure(int availableWidth, int availableHeight)
		{
			return RealControl != null ? RealControl.Measure(availableWidth, availableHeight) : base.Measure(availableHeight, availableHeight);
		}

		async void UpdateSource(bool initialize)
		{
			if (initialize && Element.Source == default(ImageSource))
				return;

			ImageSource source = Element.Source;
			((IImageController)Element).SetIsLoading(true);

			if (RealControl != null)
			{
				bool success = await RealControl.LoadFromImageSourceAsync(source);
				if (!IsDisposed && success)
				{
					((IVisualElementController)Element).NativeSizeChanged();
					UpdateAfterLoading(initialize);
				}
			}

			if (!IsDisposed)
				((IImageController)Element).SetIsLoading(false);
		}

		void UpdateFile(bool initialize)
		{
			if (initialize && Specific.GetFile(Element) == default || Element.Source != default(ImageSource))
				return;

			if (RealControl != null)
			{
				bool success = RealControl.LoadFromFile(Specific.GetFile(Element));
				if (!IsDisposed && success)
				{
					((IVisualElementController)Element).NativeSizeChanged();
					UpdateAfterLoading(initialize);
				}
			}
		}

		protected virtual void UpdateAfterLoading(bool initialize)
		{
			UpdateIsOpaque(initialize);
			UpdateBlendColor(initialize);
			UpdateIsAnimationPlaying(initialize);
		}

		void UpdateAspect(bool initialize)
		{
			if (initialize && Element.Aspect == Aspect.AspectFit)
				return;

			RealControl.ApplyAspect(Element.Aspect);
		}

		void UpdateIsOpaque(bool initialize)
		{
			if (initialize && !Element.IsOpaque)
				return;

			RealControl.IsOpaque = Element.IsOpaque;
		}

		void UpdateIsAnimationPlaying(bool initialize)
		{
			if (initialize && !Element.IsAnimationPlaying)
				return;

			RealControl.IsAnimated = Element.IsAnimationPlaying;
			RealControl.IsAnimationPlaying = Element.IsAnimationPlaying;
		}

		void UpdateBlendColor(bool initialize)
		{
			if (initialize && Specific.GetBlendColor(Element).IsDefault)
				return;

			RealControl.Color = Specific.GetBlendColor(Element).ToNative();
		}
	}
}
