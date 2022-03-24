using System.Threading.Tasks;
using Microsoft.Maui.Devices;
using Xunit;

namespace Tests
{
	public class Contacts_Tests
	{
		[Fact]
		public async Task Contacts_GetAll() =>
			await Assert.ThrowsAsync<NotImplementedInReferenceAssemblyException>(() => Contacts.GetAllAsync());
	}
}
