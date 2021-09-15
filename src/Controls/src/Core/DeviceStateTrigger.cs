using Microsoft.Maui.Essentials;

namespace Microsoft.Maui.Controls
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
			SetActive(DevicePlatform.Create(Device) == DeviceInfo.Platform);
		}
	}
}