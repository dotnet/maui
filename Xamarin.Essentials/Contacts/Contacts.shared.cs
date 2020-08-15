using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Contacts
    {
        public static Task<Contact?> PickContactAsync() =>
            PlatformPickContactAsync();
    }

    public enum ContactType
    {
        Unknown = 0,
        Personal = 1,
        Work = 2
    }
}
