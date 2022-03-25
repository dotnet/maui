using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.Communication;
using Xunit;

namespace Tests
{
	public class Sms_Tests
	{
		[Fact]
		public Task Sms_Fail_On_NetStandard() =>
			Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => Sms.ComposeAsync());
	}
}
