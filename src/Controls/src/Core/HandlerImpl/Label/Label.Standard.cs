#nullable disable
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../../docs/Microsoft.Maui.Controls/Label.xml" path="Type[@FullName='Microsoft.Maui.Controls.Label']/Docs/*" />
	public partial class Label
	{
		public static void MapTextType(ILabelHandler handler, Label label) { }
		public static void MapText(ILabelHandler handler, Label label) { }

		public static void MapLineBreakMode(ILabelHandler handler, Label label) { }
		public static void MapMaxLines(ILabelHandler handler, Label label) { }

		public static void MapTextType(LabelHandler handler, Label label) => MapText((ILabelHandler)handler, label);
		public static void MapText(LabelHandler handler, Label label) => MapText((ILabelHandler)handler, label);
		public static void MapLineBreakMode(LabelHandler handler, Label label) => MapLineBreakMode((ILabelHandler)handler, label);
		public static void MapMaxLines(LabelHandler handler, Label label) => MapMaxLines((ILabelHandler)handler, label);
	}
}
