using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class RadioButtonHandler : ViewHandler<IRadioButton, RadioButton>
	{
		protected override RadioButton CreatePlatformView()
		{
			// Note: We set a random GUID as the GroupName as part of the work-around in https://github.com/dotnet/maui/issues/11418
			return new RadioButton() { GroupName = Guid.NewGuid().ToString() };
		}

		protected override void ConnectHandler(RadioButton platformView)
		{
			platformView.Checked += OnCheckedOrUnchecked;
			platformView.Unchecked += OnCheckedOrUnchecked;

			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(RadioButton platformView)
		{
			platformView.Checked -= OnCheckedOrUnchecked;
			platformView.Unchecked -= OnCheckedOrUnchecked;

			base.DisconnectHandler(platformView);
		}

		public static void MapBackground(IRadioButtonHandler handler, IRadioButton radioButton)
		{
			handler.PlatformView?.UpdateBackground(radioButton);
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

		internal override bool PreventGestureBubbling => true;
	}
}