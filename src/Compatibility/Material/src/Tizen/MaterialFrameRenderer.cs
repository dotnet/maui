using Tizen.NET.MaterialComponents;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.Material.Tizen;
using Microsoft.Maui.Controls.Platform.Tizen;
using Microsoft.Maui.Controls.Platform.Tizen.Native;

[assembly: ExportRenderer(typeof(Frame), typeof(MaterialFrameRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
namespace Microsoft.Maui.Controls.Compatibility.Material.Tizen
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
