using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../../docs/Microsoft.Maui.Controls/CheckBox.xml" path="Type[@FullName='Microsoft.Maui.Controls.CheckBox']/Docs/*" />
	public partial class CheckBox : ICheckBox, IMapColorPropertyToPaint
	{
		public Paint Foreground { get; private set; }

		void IMapColorPropertyToPaint.MapColorPropertyToPaint(Color color)
		{
			Foreground = color?.AsPaint();
		}
	}
}
