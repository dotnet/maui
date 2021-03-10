using System;

namespace Microsoft.Maui.Handlers
{
	public partial class LabelHandler : AbstractViewHandler<ILabel, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();

		public static void MapText(IViewHandler handler, ILabel label) { }
		public static void MapTextColor(IViewHandler handler, ILabel label) { }
		public static void MapFontFamily(LabelHandler handler, ILabel label) { }
		public static void MapFontSize(LabelHandler handler, ILabel label) { }
		public static void MapFontAttributes(LabelHandler handler, ILabel label) { }
		public static void MapPadding(LabelHandler handler, ILabel label) { }
	}
}