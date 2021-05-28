using System;
using Gtk;

namespace Microsoft.Maui.Handlers
{

	public partial class CheckBoxHandler : ViewHandler<ICheckBox, CheckButton>
	{

		protected override CheckButton CreateNativeView() => new();

		public static void MapIsChecked(CheckBoxHandler handler, ICheckBox check)
		{
			handler.NativeView?.UpdateIsChecked(check);
		}

		protected override void ConnectHandler(CheckButton nativeView)
		{
			nativeView.Toggled += OnToggledEvent;
		}

		protected override void DisconnectHandler(CheckButton nativeView)
		{
			nativeView.Toggled -= OnToggledEvent;
		}

		protected void OnToggledEvent(object? sender, EventArgs e)
		{
			if (sender is CheckButton nativeView && VirtualView != null)
				VirtualView.IsChecked = nativeView.Active;

		}

	}

}