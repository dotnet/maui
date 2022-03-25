using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Battery.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Battery']/Docs" />
	partial class BatteryImplementation : IBattery
	{
		void StartBatteryListeners() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		void StopBatteryListeners() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public double ChargeLevel =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public BatteryState State =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public BatteryPowerSource PowerSource =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		void StartEnergySaverListeners() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		void StopEnergySaverListeners() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public EnergySaverStatus EnergySaverStatus =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
