using System;
using Tizen.UIExtensions.NUI.GraphicsView;

namespace Microsoft.Maui.Handlers
{
	public partial class CheckBoxHandler : ViewHandler<ICheckBox, CheckBox>
	{
		protected override CheckBox CreatePlatformView() => new CheckBox
		{
			Focusable = true,
		};

		protected override void ConnectHandler(CheckBox platformView)
		{
			base.ConnectHandler(platformView);
			platformView.ValueChanged += OnStateChanged;
		}

		protected override void DisconnectHandler(CheckBox platformView)
		{
			if (!platformView.HasBody())
				return;

			base.DisconnectHandler(platformView);
			platformView.ValueChanged -= OnStateChanged;
		}

		public static partial void MapIsChecked(ICheckBoxHandler handler, ICheckBox check)
		{
			handler.PlatformView?.UpdateIsChecked(check);
		}

		public static partial void MapForeground(ICheckBoxHandler handler, ICheckBox check)
		{
			handler.PlatformView?.UpdateForeground(check);
		}

		void OnStateChanged(object? sender, EventArgs e)
		{
			if (VirtualView == null)
				return;

			if (PlatformView != null)
				VirtualView.IsChecked = PlatformView.IsChecked;
		}
	}
}