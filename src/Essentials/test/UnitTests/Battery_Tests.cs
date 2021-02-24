using Microsoft.Maui.Essentials;
using Xunit;

namespace Tests
{
	public class Battery_Tests
	{
		[Fact]
		public void Charge_Level_On_NetStandard() =>
			Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Battery.ChargeLevel);

		[Fact]
		public void Charge_State_On_NetStandard() =>
			Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Battery.State);

		[Fact]
		public void Charge_Power_Source_On_NetStandard() =>
			Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Battery.PowerSource);

		[Fact]
		public void Battery_Changed_Event_On_NetStandard() =>
			Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Battery.BatteryInfoChanged += Battery_BatteryInfoChanged);

		void Battery_BatteryInfoChanged(object sender, BatteryInfoChangedEventArgs e)
		{
		}
	}
}
