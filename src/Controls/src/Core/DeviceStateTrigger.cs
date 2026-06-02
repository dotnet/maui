#nullable disable
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// A state trigger that activates when the app runs on a specified device platform.
	/// </summary>
	public sealed class DeviceStateTrigger : StateTriggerBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DeviceStateTrigger"/> class.
		/// </summary>
		public DeviceStateTrigger()
		{
		}

		/// <summary>
		/// Gets or sets the device platform name (e.g., "Android", "iOS", "WinUI"). This is a bindable property.
		/// </summary>
		public string Device
		{
			get => (string)GetValue(DeviceProperty);
			set => SetValue(DeviceProperty, value);
		}

		/// <summary>Bindable property for <see cref="Device"/>.</summary>
		public static readonly BindableProperty DeviceProperty =
			BindableProperty.Create(nameof(Device), typeof(string), typeof(DeviceStateTrigger), string.Empty,
				propertyChanged: OnDeviceChanged);

		static void OnDeviceChanged(BindableObject bindable, object oldvalue, object newvalue)
		{
			(bindable as DeviceStateTrigger)?.UpdateState();
		}

		protected override void OnAttached()
		{
			base.OnAttached();

			UpdateState();
		}

		void UpdateState()
		{
			if (string.IsNullOrEmpty(Device))
				return;

			var device = DevicePlatform.Create(Device);

			if (device == DevicePlatform.Create("UWP"))
				SetActive(DeviceInfo.Platform == DevicePlatform.WinUI);
			else
				SetActive(DeviceInfo.Platform == device);
		}
	}
}