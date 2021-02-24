using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Essentials;
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

			var list = new List<Microsoft.Maui.Essentials.Contact>();
			var contacts = await Microsoft.Maui.Essentials.Contacts.GetAllAsync();

			foreach (var contact in contacts.Take(10))
				list?.Add(contact);
		}
	}
}
