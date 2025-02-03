using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.Communication;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests.Shared
{
	using Contacts = ApplicationModel.Communication.Contacts;

	[Category("Contacts")]
	public class Contacts_Tests
	{
		[Fact]
		[Trait(Traits.InteractionType, Traits.InteractionTypes.Human)]
		public async Task GetAll()
		{
			await MainThread.InvokeOnMainThreadAsync(async () =>
			{
				await Permissions.RequestAsync<Permissions.ContactsRead>().ConfigureAwait(false);
			});

			var list = new List<Contact>();
			var contacts = await Contacts.GetAllAsync();

			foreach (var contact in contacts.Take(10))
				list?.Add(contact);
		}
	}
}
