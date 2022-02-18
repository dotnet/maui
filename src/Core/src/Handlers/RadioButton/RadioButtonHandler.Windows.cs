using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Handlers
{
	public partial class RadioButtonHandler : ViewHandler<IRadioButton, MauiRadioButton>
	{
		protected override MauiRadioButton CreatePlatformView() => new MauiRadioButton();

		protected override void ConnectHandler(MauiRadioButton platformView)
		{
			platformView.Checked += OnCheckedOrUnchecked;
			platformView.Unchecked += OnCheckedOrUnchecked;

			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(MauiRadioButton platformView)
		{
			platformView.Checked -= OnCheckedOrUnchecked;
			platformView.Unchecked -= OnCheckedOrUnchecked;

			base.DisconnectHandler(platformView);
		}

		public static void MapIsChecked(RadioButtonHandler handler, IRadioButton radioButton)
		{
			handler.PlatformView?.UpdateIsChecked(radioButton);
		}

		public static void MapTextColor(RadioButtonHandler handler, ITextStyle textStyle) =>
			handler.PlatformView?.UpdateTextColor(textStyle);

		public static void MapCharacterSpacing(RadioButtonHandler handler, ITextStyle textStyle) =>
			handler.PlatformView?.UpdateCharacterSpacing(textStyle);

		public static void MapContent(RadioButtonHandler handler, IRadioButton radioButton) =>
			handler.PlatformView?.UpdateContent(radioButton);

		public static void MapFont(RadioButtonHandler handler, ITextStyle button)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();
			handler.PlatformView?.UpdateFont(button, fontManager);
		}

		public static void MapStrokeColor(RadioButtonHandler handler, IRadioButton radioButton) =>
			handler.PlatformView?.UpdateStrokeColor(radioButton);

		public static void MapStrokeThickness(RadioButtonHandler handler, IRadioButton radioButton) =>
			handler.PlatformView?.UpdateStrokeThickness(radioButton);

		public static void MapCornerRadius(RadioButtonHandler handler, IRadioButton radioButton) =>
			handler.PlatformView?.UpdateCornerRadius(radioButton);

		void OnCheckedOrUnchecked(object? sender, RoutedEventArgs e)
		{
			if (VirtualView == null || PlatformView == null)
			{
				return;
			}

			VirtualView.IsChecked = PlatformView.IsChecked == true;
		}
	}
}