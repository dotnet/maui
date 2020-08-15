using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Contacts
    {
        static Task<Contact?> PlatformPickContactAsync() => throw ExceptionUtils.NotSupportedOrImplementedException;
    }
}
