using System;
using ElmSharp;

namespace Microsoft.Maui.Handlers
{
	public partial class RadioButtonHandler : ViewHandler<IRadioButton, MauiRadioButton>
	{
		protected override MauiRadioButton CreatePlatformView()
		{
			_ = NativeParent ?? throw new ArgumentNullException(nameof(NativeParent));

			return new MauiRadioButton(NativeParent)
			{
				StateValue = 1
			};
		}

		protected override void ConnectHandler(MauiRadioButton platformView)
		{
			PlatformView.ValueChanged += OnValueChanged;
			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(MauiRadioButton platformView)
		{
			PlatformView.ValueChanged -= OnValueChanged;
			base.DisconnectHandler(platformView);
		}

		public static void MapIsChecked(RadioButtonHandler handler, IRadioButton radioButton)
		{
			handler.PlatformView?.UpdateIsChecked(radioButton);
		}

		[MissingMapper]
		public static void MapContent(RadioButtonHandler handler, IRadioButton radioButton) { }

		public static void MapTextColor(RadioButtonHandler handler, ITextStyle textStyle)
		{
			handler.PlatformView?.UpdateTextColor(textStyle);
		}

		[MissingMapper]
		public static void MapCharacterSpacing(RadioButtonHandler handler, ITextStyle textStyle) { }

		[MissingMapper]
		public static void MapFont(RadioButtonHandler handler, ITextStyle textStyle) { }

		[MissingMapper]
		public static void MapStrokeColor(RadioButtonHandler handler, IRadioButton radioButton) { }

		[MissingMapper]
		public static void MapStrokeThickness(RadioButtonHandler handler, IRadioButton radioButton) { }

		[MissingMapper]
		public static void MapCornerRadius(RadioButtonHandler handler, IRadioButton radioButton) { }

		void OnValueChanged(object? sender, EventArgs e)
		{
			VirtualView.IsChecked = PlatformView.GroupValue == 1 ? true : false;
		}
	}
}