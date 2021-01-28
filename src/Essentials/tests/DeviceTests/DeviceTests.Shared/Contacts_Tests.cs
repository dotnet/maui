using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xunit;

namespace DeviceTests.Shared
{
    public class Contacts_Tests
    {
        [Fact]
        [Trait(Traits.InteractionType, Traits.InteractionTypes.Human)]
        public async Task GetAll()
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Permissions.RequestAsync<Permissions.ContactsRead>();
            });

            var list = new List<Xamarin.Essentials.Contact>();
            var contacts = await Xamarin.Essentials.Contacts.GetAllAsync();

            foreach (var contact in contacts)
                list?.Add(contact);
        }
    }
}
