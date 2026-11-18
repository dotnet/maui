using System;
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
			if (platformView is MaterialCheckBox materialCheckBox)
			{
				materialCheckBox.CheckedStateChanged += OnCheckedStateChanged;
			}
			else
			{
				platformView.CheckedChange += OnCheckedChange;
			}
		}

		protected override void DisconnectHandler(AppCompatCheckBox platformView)
		{
			if (platformView is MaterialCheckBox materialCheckBox)
			{
				materialCheckBox.CheckedStateChanged -= OnCheckedStateChanged;
			}
			else
			{
				platformView.CheckedChange -= OnCheckedChange;
			}
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

		public static partial void MapIsThreeState(ICheckBoxHandler handler, ICheckBox check) { }

		public static partial void MapForeground(ICheckBoxHandler handler, ICheckBox check)
		{
			handler.PlatformView?.UpdateForeground(check);
		}

		void OnCheckedChange(object? sender, CompoundButton.CheckedChangeEventArgs e)
		{
			if (VirtualView != null)
				SetCheckedState(VirtualView, e.IsChecked);
		}

		void OnCheckedStateChanged(object? sender, EventArgs e)
		{
			if (sender is MaterialCheckBox platformView && VirtualView != null)
			{
				SetCheckedState(VirtualView, platformView.CheckedState switch
				{
					MaterialCheckBox.StateChecked => true,
					MaterialCheckBox.StateIndeterminate => null,
					_ => false
				});
			}
		}
	}
}