#nullable enable
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class LabelHandler : ViewHandler<ILabel, FrameworkElement>
	{
		protected TextBlock? RealNativeView { get; set; }

		protected override FrameworkElement CreateNativeView()
		{
			RealNativeView = new TextBlock();
			return new Border { Child = RealNativeView };
		}

		public static void MapText(LabelHandler handler, ILabel label) =>
			handler.RealNativeView?.UpdateText(label);

		public static void MapTextColor(LabelHandler handler, ILabel label) =>
			handler.RealNativeView?.UpdateTextColor(label);

		public static void MapCharacterSpacing(LabelHandler handler, ILabel label) =>	
			handler.RealNativeView?.UpdateCharacterSpacing(label);

		public static void MapFont(LabelHandler handler, ILabel label)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.RealNativeView?.UpdateFont(label, fontManager);
		}

		public static void MapHorizontalTextAlignment(LabelHandler handler, ILabel label) => 
			handler.RealNativeView?.UpdateHorizontalTextAlignment(label);

		public static void MapLineBreakMode(LabelHandler handler, ILabel label) => 
			handler.RealNativeView?.UpdateLineBreakMode(label);

		public static void MapTextDecorations(LabelHandler handler, ILabel label) =>	
			handler.RealNativeView?.UpdateTextDecorations(label);

		public static void MapMaxLines(LabelHandler handler, ILabel label) =>
			handler.RealNativeView?.UpdateMaxLines(label);

		public static void MapPadding(LabelHandler handler, ILabel label) =>
			handler.RealNativeView?.UpdatePadding(label);

		public static void MapLineHeight(LabelHandler handler, ILabel label) =>		
			handler.RealNativeView?.UpdateLineHeight(label);
	}
}
