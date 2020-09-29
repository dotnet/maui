using Tizen.NET.MaterialComponents;
using Xamarin.Forms;
using Xamarin.Forms.Material.Tizen;
using Xamarin.Forms.Platform.Tizen;
using Xamarin.Forms.Platform.Tizen.Native;

[assembly: ExportRenderer(typeof(Frame), typeof(MaterialFrameRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
namespace Xamarin.Forms.Material.Tizen
{
	public class MaterialFrameRenderer : ViewRenderer<Frame, MCard>
	{
		public MaterialFrameRenderer()
		{
			RegisterPropertyHandler(Frame.BorderColorProperty, UpdateBorderColor);
			RegisterPropertyHandler(Frame.HasShadowProperty, UpdateShadowVisibility);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Frame> e)
		{
			if (Control == null)
			{
				SetNativeControl(new MaterialCanvas(Forms.NativeParent));
			}
			base.OnElementChanged(e);
		}

		void UpdateBorderColor()
		{
			Control.BorderColor = Element.BorderColor.ToNative();
		}

		void UpdateShadowVisibility()
		{
			Control.HasShadow = Element.HasShadow;
		}
	}
}
