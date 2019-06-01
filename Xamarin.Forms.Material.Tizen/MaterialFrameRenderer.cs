using Xamarin.Forms;
using Xamarin.Forms.Platform.Tizen;
using Xamarin.Forms.Platform.Tizen.Native;
using Xamarin.Forms.Material.Tizen;
using Tizen.NET.MaterialComponents;
using TForms = Xamarin.Forms.Platform.Tizen.Forms;

[assembly: ExportRenderer(typeof(Frame), typeof(MaterialFrameRenderer), new[] { typeof(VisualMarker.MaterialVisual) })]
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
				SetNativeControl(new MaterialCanvas(TForms.NativeParent));
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
