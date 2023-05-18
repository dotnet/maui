#nullable disable
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/DeviceStateTrigger.xml" path="Type[@FullName='Microsoft.Maui.Controls.DeviceStateTrigger']/Docs/*" />
	public sealed class DeviceStateTrigger : StateTriggerBase
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/DeviceStateTrigger.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public DeviceStateTrigger()
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/DeviceStateTrigger.xml" path="//Member[@MemberName='Device']/Docs/*" />
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