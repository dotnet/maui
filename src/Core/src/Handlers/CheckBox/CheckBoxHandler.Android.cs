using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using Google.Android.Material.CheckBox;

namespace Microsoft.Maui.Handlers
{
	public partial class CheckBoxHandler : ViewHandler<ICheckBox, AppCompatCheckBox>
	{
		protected override AppCompatCheckBox CreatePlatformView()
		{
			var platformCheckBox = new MaterialCheckBox(MauiMaterialContextThemeWrapper.Create(Context))
			{
				SoundEffectsEnabled = false
			};

			platformCheckBox.SetClipToOutline(true);
			return platformCheckBox;
		}

		protected override void ConnectHandler(AppCompatCheckBox platformView)
		{
			platformView.CheckedChange += OnCheckedChange;
		}

		protected override void DisconnectHandler(AppCompatCheckBox platformView)
		{
			platformView.CheckedChange -= OnCheckedChange;
		}

		// This is an Android-specific mapping
		public static partial void MapBackground(ICheckBoxHandler handler, ICheckBox check)
		{
			handler.PlatformView?.UpdateBackground(check);
		}

		public static partial void MapIsChecked(ICheckBoxHandler handler, ICheckBox check)
		{
			handler.PlatformView?.UpdateIsChecked(check);
		}

		public static partial void MapForeground(ICheckBoxHandler handler, ICheckBox check)
		{
			handler.PlatformView?.UpdateForeground(check);
		}

		void OnCheckedChange(object? sender, CompoundButton.CheckedChangeEventArgs e)
		{
			VirtualView?.IsChecked = e.IsChecked;
		}
	}
}