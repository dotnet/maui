using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class LabelHandler : AbstractViewHandler<ILabel, UILabel>
	{
		protected override UILabel CreateNativeView() => new UILabel();

		public static void MapText(LabelHandler handler, ILabel label)
		{
			handler.TypedNativeView?.UpdateText(label);
		}

		public static void MapTextColor(LabelHandler handler, ILabel label)
		{
			handler.TypedNativeView?.UpdateTextColor(label);
		}
	}
}