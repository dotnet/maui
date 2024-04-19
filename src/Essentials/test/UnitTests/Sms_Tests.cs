using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.Communication;
using Xunit;

namespace Tests
{
	public class Sms_Tests
	{
		[Fact]
		public async Task Sms_Fail_On_NetStandard() =>
			await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => Sms.ComposeAsync());
	}
}
