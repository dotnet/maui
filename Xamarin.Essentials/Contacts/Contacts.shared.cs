using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Contacts
    {
        public static async Task<Contact> PickContactAsync()
        {
            if (DeviceInfo.Platform != DevicePlatform.iOS)
                await Permissions.RequestAsync<Permissions.ContactsRead>();

            return await PlatformPickContactAsync();
        }
    }

    public enum ContactType
    {
        Unknown = 0,
        Personal = 1,
        Work = 2
    }
}
