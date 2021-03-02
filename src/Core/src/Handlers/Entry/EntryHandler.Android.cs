using Android.Content.Res;
using Android.Widget;

namespace Microsoft.Maui.Handlers
{
	public partial class EntryHandler : AbstractViewHandler<IEntry, EditText>
	{
		static ColorStateList? DefaultTextColors { get; set; }

		protected override EditText CreateNativeView()
		{
			return new EditText(Context);
		}

		protected override void SetupDefaults(EditText nativeView)
		{
			base.SetupDefaults(nativeView);
			DefaultTextColors = nativeView.TextColors;
		}

		public static void MapText(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdateText(entry);
		}

		public static void MapTextColor(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdateTextColor(entry, DefaultTextColors);
		}

		public static void MapIsPassword(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdateIsPassword(entry);
		}
	}
}