using Android.Widget;
using AndroidX.AppCompat.Widget;

namespace Microsoft.Maui.Handlers
{
	public partial class CheckBoxHandler : ViewHandler<ICheckBox, AppCompatCheckBox>
	{
		CheckedChangeListener ChangeListener { get; } = new CheckedChangeListener();

		protected override AppCompatCheckBox CreatePlatformView()
		{
			var platformCheckBox = new AppCompatCheckBox(Context)
			{
				SoundEffectsEnabled = false
			};

			platformCheckBox.SetClipToOutline(true);

			return platformCheckBox;
		}

		protected override void ConnectHandler(AppCompatCheckBox platformView)
		{
			ChangeListener.Handler = this;
			platformView.SetOnCheckedChangeListener(ChangeListener);
		}

		protected override void DisconnectHandler(AppCompatCheckBox platformView)
		{
			ChangeListener.Handler = null;
			platformView.SetOnCheckedChangeListener(null);
		}

		// This is an Android-specific mapping
		public static void MapBackground(CheckBoxHandler handler, ICheckBox check)
		{
			handler.PlatformView?.UpdateBackground(check);
		}

		public static void MapIsChecked(CheckBoxHandler handler, ICheckBox check)
		{
			handler.PlatformView?.UpdateIsChecked(check);
		}

		public static void MapForeground(CheckBoxHandler handler, ICheckBox check)
		{
			handler.PlatformView?.UpdateForeground(check);
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

			public void OnCheckedChanged(CompoundButton? platformCheckBox, bool isChecked)
			{
				if (Handler == null || platformCheckBox == null)
					return;

				Handler.OnCheckedChanged(isChecked);
			}
		}
	}
}