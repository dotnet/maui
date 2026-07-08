using Tizen.NET.MaterialComponents;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.Material.Tizen;
using Microsoft.Maui.Controls.Platform.Tizen;

[assembly: ExportRenderer(typeof(CheckBox), typeof(MaterialCheckBoxRenderer), new[] { typeof(VisualMarker.MaterialVisual) }, Priority = short.MinValue)]
namespace Microsoft.Maui.Controls.Compatibility.Material.Tizen
{
	public class MaterialCheckBoxRenderer : CheckBoxRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<CheckBox> e)
		{
			if (Control == null)
			{
				SetNativeControl(new MCheckBox(Forms.NativeParent));
			}
			base.OnElementChanged(e);
		}
	}
}
