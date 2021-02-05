using Xamarin.Essentials;
using Xunit;

namespace Tests
{
	public class Preferences_Tests
	{
		[Fact]
		public void Preferences_Set_Fail_On_NetStandard() =>
			Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Preferences.Set("anything", "fails"));

		[Fact]
		public void Preferences_Get_Fail_On_NetStandard()
		{
			Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Preferences.Get("anything", "fails"));
			Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Preferences.Get("anything", "fails", "shared"));
		}

		[Fact]
		public void Preferences_ContainsKey_Fail_On_NetStandard()
		{
			Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Preferences.ContainsKey("anything"));
			Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Preferences.ContainsKey("anything", "shared"));
		}

		[Fact]
		public void Preferences_Remove_Fail_On_NetStandard()
		{
			Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Preferences.Remove("anything"));
			Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Preferences.Remove("anything", "shared"));
		}

		[Fact]
		public void Preferences_Get_Clear_On_NetStandard()
		{
			Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Preferences.Clear());
			Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Preferences.Clear("shared"));
		}
	}
}
