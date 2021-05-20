using System;

namespace Microsoft.Maui.Handlers
{
	public partial class LabelHandler : ViewHandler<ILabel, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();

		public static void MapText(IFrameworkElementHandler handler, ILabel label) { }
		public static void MapTextColor(IFrameworkElementHandler handler, ILabel label) { }
		public static void MapCharacterSpacing(IFrameworkElementHandler handler, ILabel label) { }
		public static void MapFont(LabelHandler handler, ILabel label) { }
		public static void MapHorizontalTextAlignment(LabelHandler handler, ILabel label) { }
		public static void MapLineBreakMode(LabelHandler handler, ILabel label) { }
		public static void MapTextDecorations(LabelHandler handler, ILabel label) { }
		public static void MapMaxLines(IFrameworkElementHandler handler, ILabel label) { }
		public static void MapPadding(LabelHandler handler, ILabel label) { }
		public static void MapLineHeight(LabelHandler handler, ILabel label) { }
	}
}