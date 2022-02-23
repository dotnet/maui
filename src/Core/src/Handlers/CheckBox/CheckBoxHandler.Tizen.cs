using System;
using ElmSharp;

namespace Microsoft.Maui.Handlers
{
	public partial class CheckBoxHandler : ViewHandler<ICheckBox, Check>
	{
		protected override Check CreatePlatformView() => new Check(NativeParent);

		protected override void ConnectHandler(Check platformView)
		{
			base.ConnectHandler(platformView);
			platformView.StateChanged += OnStateChanged;
		}

		protected override void DisconnectHandler(Check platformView)
		{
			base.DisconnectHandler(platformView);
			platformView.StateChanged -= OnStateChanged;
		}

		public static void MapIsChecked(ICheckBoxHandler handler, ICheckBox check)
		{
			handler.PlatformView?.UpdateIsChecked(check);
		}

		public static void MapForeground(ICheckBoxHandler handler, ICheckBox check)
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