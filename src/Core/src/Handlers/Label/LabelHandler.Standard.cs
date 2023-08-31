using System;

namespace Microsoft.Maui.Handlers
{
	public partial class LabelHandler : ViewHandler<ILabel, object>
	{
		protected override object CreatePlatformView() => throw new NotImplementedException();

		public static partial void MapBackground(ILabelHandler handler, ILabel label) { }
		public static partial void MapText(ILabelHandler handler, ILabel label) { }
		public static partial void MapTextColor(ILabelHandler handler, ILabel label) { }
		public static partial void MapCharacterSpacing(ILabelHandler handler, ILabel label) { }
		public static partial void MapFont(ILabelHandler handler, ILabel label) { }
		public static partial void MapHorizontalTextAlignment(ILabelHandler handler, ILabel label) { }
		public static partial void MapVerticalTextAlignment(ILabelHandler handler, ILabel label) { }
		public static partial void MapTextDecorations(ILabelHandler handler, ILabel label) { }
		public static partial void MapPadding(ILabelHandler handler, ILabel label) { }
		public static partial void MapLineHeight(ILabelHandler handler, ILabel label) { }
	}
}