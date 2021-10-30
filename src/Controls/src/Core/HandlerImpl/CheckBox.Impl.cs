using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public partial class CheckBox : ICheckBox, IMapColorPropertyToPaint
	{
		public Paint Foreground { get; private set; }

		void IMapColorPropertyToPaint.MapColorPropertyToPaint(Color color)
		{
			Foreground = color?.AsPaint();
		}
	}
}