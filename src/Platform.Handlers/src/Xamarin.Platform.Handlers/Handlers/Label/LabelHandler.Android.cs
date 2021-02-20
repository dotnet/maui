using Android.Widget;
using Xamarin.Forms;

namespace Xamarin.Platform.Handlers
{
	public partial class LabelHandler : AbstractViewHandler<ILabel, TextView>
	{
		static Color DefaultTextColor { get; set; }

		protected override TextView CreateNativeView() => new TextView(Context);

		protected override void SetupDefaults(TextView nativeView)
		{
			if (nativeView.TextColors == null)
			{
				DefaultTextColor = Color.Default;
			}
			else
			{
				DefaultTextColor = Color.FromUint((uint)nativeView.TextColors.DefaultColor);
			}
		}

		public static void MapText(LabelHandler handler, ILabel label)
		{
			ViewHandler.CheckParameters(handler, label);

			handler.TypedNativeView?.UpdateText(label);
		}

		public static void MapTextColor(LabelHandler handler, ILabel label)
		{
			ViewHandler.CheckParameters(handler, label);

			handler.TypedNativeView?.UpdateTextColor(label, DefaultTextColor);
		}
	}
}