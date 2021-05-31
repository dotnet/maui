using System;
using ElmSharp;

namespace Microsoft.Maui.Handlers
{
	public partial class CheckBoxHandler : EViewHandler<ICheckBox, Check>
	{
		protected override Check CreateNativeView() => new Check(NativeParent);

		protected override void ConnectHandler(Check nativeView)
		{
			base.ConnectHandler(nativeView);
			nativeView.StateChanged += OnStateChanged;
		}

		protected override void DisconnectHandler(Check nativeView)
		{
			base.DisconnectHandler(nativeView);
			nativeView.StateChanged -= OnStateChanged;
		}

		public static void MapIsChecked(CheckBoxHandler handler, ICheckBox check)
		{
			handler.NativeView?.UpdateIsChecked(check);
		}

		public static void MapForeground(CheckBoxHandler handler, ICheckBox check)
		{
			handler.NativeView?.UpdateForeground(check);
		}

		void OnStateChanged(object? sender, EventArgs e)
		{
			if (VirtualView == null)
				return;

			if (NativeView != null)
				VirtualView.IsChecked = NativeView.IsChecked;
		}
	}
}