using System.Threading.Tasks;
using Xamarin.Essentials;
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
