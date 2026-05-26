#nullable disable
using Android.Content;
using Android.Widget;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public class SwitchCellView : BaseCellView, CompoundButton.IOnCheckedChangeListener
	{
#pragma warning disable CS0618 // Type or member is obsolete
		public SwitchCellView(Context context, Cell cell) : base(context, cell)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			var sw = new global::Android.Widget.Switch(context);
			sw.SetOnCheckedChangeListener(this);

			SetAccessoryView(sw);

			SetImageVisible(false);
		}

#pragma warning disable CS0618 // Type or member is obsolete
		public SwitchCell Cell { get; set; }
#pragma warning restore CS0618 // Type or member is obsolete

		public void OnCheckedChanged(CompoundButton buttonView, bool isChecked)
		{
			Cell.On = isChecked;
		}
	}
}