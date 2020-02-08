using FormsDevice = Xamarin.Forms.Device;

namespace Xamarin.Forms
{
	public sealed class DeviceStateTrigger : StateTriggerBase
	{
		public DeviceStateTrigger()
		{
			UpdateState();
		}

		public string Device
		{
			get => (string)GetValue(DeviceProperty);
			set => SetValue(DeviceProperty, value);
		}

		public static readonly BindableProperty DeviceProperty =
		BindableProperty.Create(nameof(Device), typeof(string), typeof(DeviceStateTrigger), string.Empty,
			propertyChanged: OnDeviceChanged);

		static void OnDeviceChanged(BindableObject bindable, object oldvalue, object newvalue)
		{
			((DeviceStateTrigger)bindable).UpdateState();
		}

		void UpdateState()
		{
			switch (Device)
			{
				case FormsDevice.Android:
					SetActive(FormsDevice.RuntimePlatform == FormsDevice.Android);
					break;
				case FormsDevice.iOS:
					SetActive(FormsDevice.RuntimePlatform == FormsDevice.iOS);
					break;
				case FormsDevice.UWP:
					SetActive(FormsDevice.RuntimePlatform == FormsDevice.UWP);
					break;
			}
		}
	}
}