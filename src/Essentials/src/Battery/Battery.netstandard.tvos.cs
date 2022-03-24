using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Battery.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Battery']/Docs" />
	public partial class BatteryImplementation : IBattery
	{
		public void StartBatteryListeners() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public void StopBatteryListeners() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public double ChargeLevel =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public BatteryState State =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public BatteryPowerSource PowerSource =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public void StartEnergySaverListeners() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public void StopEnergySaverListeners() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public EnergySaverStatus EnergySaverStatus =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
