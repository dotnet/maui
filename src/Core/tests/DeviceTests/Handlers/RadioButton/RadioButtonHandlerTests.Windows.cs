namespace Microsoft.Maui.DeviceTests
{
    public partial class RadioButtonHandlerTests
    {
        UI.Xaml.Controls.RadioButton GetNativeRadioButton(RadioButtonHandler radioButtonHandler) =>
            radioButtonHandler.NativeView;

        bool GetNativeIsChecked(RadioButtonHandler radioButtonHandler) =>
            GetNativeRadioButton(radioButtonHandler).IsChecked ?? false;
    }
}
