using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/CheckBox.xml" path="Type[@FullName='Microsoft.Maui.Controls.CheckBox']/Docs" />
	public partial class CheckBox : ICheckBox, IMapColorPropertyToPaint
	{
		Paint Foreground { get; set; }

		void IMapColorPropertyToPaint.MapColorPropertyToPaint(Color color)
		{
			Foreground = color?.AsPaint();
			Handler?.UpdateValue(nameof(ICheckBox.Foreground));
		}

		Paint ICheckBox.Foreground =>
				Foreground ?? (DefaultStyles.GetColor(this, ColorProperty)?.Value as Color)?.AsPaint();
	}
}