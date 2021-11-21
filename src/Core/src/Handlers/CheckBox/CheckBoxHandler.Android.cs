using Android.Widget;
using AndroidX.AppCompat.Widget;

namespace Microsoft.Maui.Handlers
{
	public partial class CheckBoxHandler : ViewHandler<ICheckBox, AppCompatCheckBox>
	{
		CheckedChangeListener ChangeListener { get; } = new CheckedChangeListener();

		protected override AppCompatCheckBox CreateNativeView()
		{
			var nativeCheckBox = new AppCompatCheckBox(Context)
			{
				SoundEffectsEnabled = false
			};

			nativeCheckBox.SetClipToOutline(true);

			return nativeCheckBox;
		}

		protected override void ConnectHandler(AppCompatCheckBox nativeView)
		{
			ChangeListener.Handler = this;
			nativeView.SetOnCheckedChangeListener(ChangeListener);
		}

		protected override void DisconnectHandler(AppCompatCheckBox nativeView)
		{
			ChangeListener.Handler = null;
			nativeView.SetOnCheckedChangeListener(null);
		}

		// This is an Android-specific mapping
		public static void MapBackground(CheckBoxHandler handler, ICheckBox check)
		{
			handler.NativeView?.UpdateBackground(check);
		}

		public static void MapIsChecked(CheckBoxHandler handler, ICheckBox check)
		{
			handler.NativeView?.UpdateIsChecked(check);
		}

		public static void MapForeground(CheckBoxHandler handler, ICheckBox check)
		{
			handler.NativeView?.UpdateForeground(check);
		}

		void OnCheckedChanged(bool isChecked)
		{
			if (VirtualView != null)
				VirtualView.IsChecked = isChecked;
		}

		internal class CheckedChangeListener : Java.Lang.Object, CompoundButton.IOnCheckedChangeListener
		{
			public CheckBoxHandler? Handler { get; set; }

			public CheckedChangeListener()
			{
			}

			public void OnCheckedChanged(CompoundButton? nativeCheckBox, bool isChecked)
			{
				if (Handler == null || nativeCheckBox == null)
					return;

				Handler.OnCheckedChanged(isChecked);
			}
		}
	}
}