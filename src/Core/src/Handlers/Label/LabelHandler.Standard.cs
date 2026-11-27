using System;

namespace Microsoft.Maui.Handlers
{
	public partial class LabelHandler : ViewHandler<ILabel, object>
	{
		protected override object CreatePlatformView() => throw new NotImplementedException();

		public static void MapText(ILabelHandler handler, ILabel label) { }
		public static void MapTextColor(ILabelHandler handler, ILabel label) { }
		public static void MapCharacterSpacing(ILabelHandler handler, ILabel label) { }
		public static void MapFont(ILabelHandler handler, ILabel label) { }
		public static void MapHorizontalTextAlignment(ILabelHandler handler, ILabel label) { }
		public static void MapVerticalTextAlignment(ILabelHandler handler, ILabel label) { }
		public static void MapTextDecorations(ILabelHandler handler, ILabel label) { }
		public static void MapMaxLines(ILabelHandler handler, ILabel label) { }
		public static void MapPadding(ILabelHandler handler, ILabel label) { }
		public static void MapLineHeight(ILabelHandler handler, ILabel label) { }
	}
}