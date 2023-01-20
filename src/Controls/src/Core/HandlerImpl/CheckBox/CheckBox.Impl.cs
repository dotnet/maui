#nullable disable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../../docs/Microsoft.Maui.Controls/CheckBox.xml" path="Type[@FullName='Microsoft.Maui.Controls.CheckBox']/Docs/*" />
	public partial class CheckBox : ICheckBox
	{
		public Paint Foreground => Color?.AsPaint();
	}
}