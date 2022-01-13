using System;
using ElmSharp;

namespace Microsoft.Maui.Handlers
{
	public partial class RadioButtonHandler : ViewHandler<IRadioButton, MauiRadioButton>
	{
		protected override MauiRadioButton CreateNativeView()
		{
			_ = NativeParent ?? throw new ArgumentNullException(nameof(NativeParent));

			return new MauiRadioButton(NativeParent)
			{
				StateValue = 1
			};
		}

		protected override void ConnectHandler(MauiRadioButton nativeView)
		{
			NativeView.ValueChanged += OnValueChanged;
			base.ConnectHandler(nativeView);
		}

		protected override void DisconnectHandler(MauiRadioButton nativeView)
		{
			NativeView.ValueChanged -= OnValueChanged;
			base.DisconnectHandler(nativeView);
		}

		public static void MapIsChecked(RadioButtonHandler handler, IRadioButton radioButton)
		{
			handler.NativeView?.UpdateIsChecked(radioButton);
		}

		[MissingMapper]
		public static void MapContent(RadioButtonHandler handler, IRadioButton radioButton) { }

		public static void MapTextColor(RadioButtonHandler handler, ITextStyle textStyle)
		{
			handler.NativeView?.UpdateTextColor(textStyle);
		}

		[MissingMapper]
		public static void MapCharacterSpacing(RadioButtonHandler handler, ITextStyle textStyle) { }

		[MissingMapper]
		public static void MapFont(RadioButtonHandler handler, ITextStyle textStyle) { }

		void OnValueChanged(object? sender, EventArgs e)
		{
			VirtualView.IsChecked = NativeView.GroupValue == 1 ? true : false;
		}
	}
}