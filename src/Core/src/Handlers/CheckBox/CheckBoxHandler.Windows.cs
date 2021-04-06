using System;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class CheckBoxHandler : ViewHandler<ICheckBox, CheckBox>
	{
		protected override CheckBox CreateNativeView() => new CheckBox();

		protected override void ConnectHandler(CheckBox nativeView)
		{
			base.ConnectHandler(nativeView);

			nativeView.CheckedChanged += OnCheckedChanged;
		}

		protected override void DisconnectHandler(CheckBox nativeView)
		{
			base.DisconnectHandler(nativeView);

			nativeView.CheckedChanged -= OnCheckedChanged;
		}

		public static void MapIsChecked(CheckBoxHandler handler, ICheckBox check)
		{
			handler.NativeView?.UpdateIsChecked(check);
		}
	}
}