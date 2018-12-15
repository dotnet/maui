using System.Threading.Tasks;
using Contacts;
using Foundation;

namespace Xamarin.Essentials
{
    public static partial class Map
    {
        // macOS code is exactly the same as iOS, se we use that file directly.

        // There is one small difference, iOS uses a MKPlacemarkAddress object
        // to AddressBook in order to create a strongly typed object. On macOS,
        // and on iOS 10.0+, the Contacts is used instead.

        class MKPlacemarkAddress : DictionaryContainer
        {
            public string City
            {
                get => GetStringValue(CNPostalAddressKey.City);
                set => SetStringValue(CNPostalAddressKey.City, value);
            }

            public string Country
            {
                get => GetStringValue(CNPostalAddressKey.Country);
                set => SetStringValue(CNPostalAddressKey.Country, value);
            }

            public string CountryCode
            {
                get => GetStringValue(CNPostalAddressKey.IsoCountryCode);
                set => SetStringValue(CNPostalAddressKey.IsoCountryCode, value);
            }

            public string State
            {
                get => GetStringValue(CNPostalAddressKey.State);
                set => SetStringValue(CNPostalAddressKey.State, value);
            }

            public string Street
            {
                get => GetStringValue(CNPostalAddressKey.Street);
                set => SetStringValue(CNPostalAddressKey.Street, value);
            }

            public string Zip
            {
                get => GetStringValue(CNPostalAddressKey.PostalCode);
                set => SetStringValue(CNPostalAddressKey.PostalCode, value);
            }

            public static implicit operator NSDictionary(MKPlacemarkAddress address) =>
                address.Dictionary;
        }
    }
}
