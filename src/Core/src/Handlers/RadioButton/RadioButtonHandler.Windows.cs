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

		public static void MapIsChecked(IRadioButtonHandler handler, IRadioButton radioButton)
		{
			handler.PlatformView?.UpdateIsChecked(radioButton);
		}

		public static void MapTextColor(IRadioButtonHandler handler, ITextStyle textStyle) =>
			handler.PlatformView?.UpdateTextColor(textStyle);

		public static void MapCharacterSpacing(IRadioButtonHandler handler, ITextStyle textStyle) =>
			handler.PlatformView?.UpdateCharacterSpacing(textStyle);

		public static void MapContent(IRadioButtonHandler handler, IRadioButton radioButton) =>
			handler.PlatformView?.UpdateContent(radioButton);

		public static void MapFont(IRadioButtonHandler handler, ITextStyle button)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();
			handler.PlatformView?.UpdateFont(button, fontManager);
		}

		public static void MapStrokeColor(IRadioButtonHandler handler, IRadioButton radioButton) =>
			handler.PlatformView?.UpdateStrokeColor(radioButton);

		public static void MapStrokeThickness(IRadioButtonHandler handler, IRadioButton radioButton) =>
			handler.PlatformView?.UpdateStrokeThickness(radioButton);

		public static void MapCornerRadius(IRadioButtonHandler handler, IRadioButton radioButton) =>
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