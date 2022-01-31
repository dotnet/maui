using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Handlers
{
	public partial class RadioButtonHandler : ViewHandler<IRadioButton, MauiRadioButton>
	{
		protected override MauiRadioButton CreateNativeView() => new MauiRadioButton();

		protected override void ConnectHandler(MauiRadioButton nativeView)
		{
			nativeView.Checked += OnCheckedOrUnchecked;
			nativeView.Unchecked += OnCheckedOrUnchecked;

			base.ConnectHandler(nativeView);
		}

		protected override void DisconnectHandler(MauiRadioButton nativeView)
		{
			nativeView.Checked -= OnCheckedOrUnchecked;
			nativeView.Unchecked -= OnCheckedOrUnchecked;

			base.DisconnectHandler(nativeView);
		}

		public static void MapIsChecked(RadioButtonHandler handler, IRadioButton radioButton)
		{
			handler.NativeView?.UpdateIsChecked(radioButton);
		}

		public static void MapTextColor(RadioButtonHandler handler, ITextStyle textStyle) =>
			handler.NativeView?.UpdateTextColor(textStyle);

		public static void MapCharacterSpacing(RadioButtonHandler handler, ITextStyle textStyle) =>
			handler.NativeView?.UpdateCharacterSpacing(textStyle);

		public static void MapContent(RadioButtonHandler handler, IRadioButton radioButton) =>
			handler.NativeView?.UpdateContent(radioButton);

		public static void MapFont(RadioButtonHandler handler, ITextStyle button)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();
			handler.NativeView?.UpdateFont(button, fontManager);
		}

		public static void MapStrokeColor(RadioButtonHandler handler, IRadioButton radioButton) =>
			handler.NativeView?.UpdateStrokeColor(radioButton);

		public static void MapStrokeThickness(RadioButtonHandler handler, IRadioButton radioButton) =>
			handler.NativeView?.UpdateStrokeThickness(radioButton);

		public static void MapCornerRadius(RadioButtonHandler handler, IRadioButton radioButton) =>
			handler.NativeView?.UpdateCornerRadius(radioButton);

		void OnCheckedOrUnchecked(object? sender, RoutedEventArgs e)
		{
			if (VirtualView == null || NativeView == null)
			{
				return;
			}

			VirtualView.IsChecked = NativeView.IsChecked == true;
		}
	}
}