using FormsDevice = Microsoft.Maui.Controls.Device;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/DeviceStateTrigger.xml" path="Type[@FullName='Microsoft.Maui.Controls.DeviceStateTrigger']/Docs" />
	public sealed class DeviceStateTrigger : StateTriggerBase
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/DeviceStateTrigger.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public DeviceStateTrigger()
		{
			UpdateState();
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/DeviceStateTrigger.xml" path="//Member[@MemberName='Device']/Docs" />
		public string Device
		{
			get => (string)GetValue(DeviceProperty);
			set => SetValue(DeviceProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/DeviceStateTrigger.xml" path="//Member[@MemberName='DeviceProperty']/Docs" />
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