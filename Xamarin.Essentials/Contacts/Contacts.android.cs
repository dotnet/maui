using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Provider;
using Net = Android.Net;

namespace Xamarin.Essentials
{
    public static partial class Contacts
    {
        static async Task<Contact> PlatformPickContactAsync()
        {
            using var intent = new Intent(Intent.ActionPick);
            intent.SetType(ContactsContract.CommonDataKinds.Phone.ContentType);
            var result = await IntermediateActivity.StartAsync(intent, Platform.requestCodePickContact).ConfigureAwait(false);

            if (result?.Data != null)
                return PlatformGetContacts(result.Data);

            return null;
        }

        internal static Contact PlatformGetContacts(Net.Uri contactUri)
        {
            if (contactUri == null)
                return default;

            using var context = Platform.AppContext.ContentResolver;

            using var cur = context.Query(contactUri, null, null, null, null);
            var emails = new List<ContactEmail>();
            var phones = new List<ContactPhone>();
            var bDate = string.Empty;

            if (cur.MoveToFirst())
            {
                var typeOfContact = cur.GetString(cur.GetColumnIndex(ContactsContract.CommonDataKinds.Phone.InterfaceConsts.Type));

                var name = cur.GetString(cur.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.DisplayName));
                var id = cur.GetString(cur.GetColumnIndex(ContactsContract.CommonDataKinds.Email.InterfaceConsts.ContactId));
                var idQ = new string[1] { id };
                global::Android.Database.ICursor cursor = null;

                GetPhones(ref idQ, ref cursor, phones, context);

                GetEmails(ref idQ, ref cursor, emails, context);

                GetBirthday(ref idQ, ref cursor, ref bDate, context);

                DateTime? birthday = default;
                if (DateTime.TryParse(bDate, out var b))
                    birthday = b;
                cursor?.Dispose();

                return new Contact(name, phones, emails, birthday, GetPhoneContactType(typeOfContact));
            }

            return default;

            static void GetPhones(ref string[] idQ, ref global::Android.Database.ICursor cursor, List<ContactPhone> phones, ContentResolver context)
            {
                var projection = new string[2]
                {
                    ContactsContract.CommonDataKinds.Phone.Number,
                    ContactsContract.CommonDataKinds.Phone.InterfaceConsts.Type,
                };

                cursor = context.Query(
                   ContactsContract.CommonDataKinds.Phone.ContentUri,
                   null,
                   ContactsContract.CommonDataKinds.Phone.InterfaceConsts.ContactId + "=?",
                   idQ,
                   null);

                if (cursor.MoveToFirst())
                {
                    do
                    {
                        var phone = cursor.GetString(cursor.GetColumnIndex(projection[0]));
                        var phoneType = cursor.GetString(cursor.GetColumnIndex(projection[1]));

                        var contactType = GetPhoneContactType(phoneType);

                        phones.Add(new ContactPhone(phone, contactType));
                    }
                    while (cursor.MoveToNext());
                }
                cursor.Close();
            }

            static void GetEmails(ref string[] idQ, ref global::Android.Database.ICursor cursor, List<ContactEmail> emails, ContentResolver context)
            {
                var projection = new string[2]
                {
                    ContactsContract.CommonDataKinds.Email.Address,
                    ContactsContract.CommonDataKinds.Email.InterfaceConsts.Type
                };

                cursor = context.Query(ContactsContract.CommonDataKinds.Email.ContentUri, null, ContactsContract.CommonDataKinds.Email.InterfaceConsts.ContactId + "=?", idQ, null);

                while (cursor.MoveToNext())
                {
                    var email = cursor.GetString(cursor.GetColumnIndex(projection[0]));
                    var emailType = cursor.GetString(cursor.GetColumnIndex(projection[1]));

                    var contactType = GetEmailContactType(emailType);

                    emails.Add(new ContactEmail(email, contactType));
                }

                cursor.Close();
            }

            static void GetBirthday(ref string[] idQ, ref global::Android.Database.ICursor cursor, ref string bDate, ContentResolver context)
            {
                var projection = new string[2]
                {
                     ContactsContract.Contacts.InterfaceConsts.Id,
                     ContactsContract.CommonDataKinds.Event.StartDate
                };

                var query = ContactsContract.CommonDataKinds.CommonColumns.Type + "=" + 3;

                cursor = context.Query(ContactsContract.Data.ContentUri, null, query, idQ, null);

                if (cursor.MoveToFirst())
                    bDate = cursor.GetString(cursor.GetColumnIndex(ContactsContract.CommonDataKinds.Event.StartDate));

                cursor.Close();
            }
        }

        static ContactType GetPhoneContactType(string type)
        {
            if (int.TryParse(type, out var typeInt))
            {
                try
                {
                    var phoneKind = (PhoneDataKind)typeInt;
                    return phoneKind switch
                    {
                        PhoneDataKind.Home => ContactType.Personal,
                        PhoneDataKind.Mobile => ContactType.Personal,
                        PhoneDataKind.Main => ContactType.Personal,
                        PhoneDataKind.Work => ContactType.Work,
                        PhoneDataKind.WorkMobile => ContactType.Work,
                        PhoneDataKind.CompanyMain => ContactType.Work,
                        PhoneDataKind.WorkPager => ContactType.Work,
                        _ => ContactType.Unknown
                    };
                }
                catch (Exception)
                {
                    return ContactType.Unknown;
                }
            }
            return ContactType.Unknown;
        }

        static ContactType GetEmailContactType(string type)
        {
            if (int.TryParse(type, out var typeInt))
            {
                try
                {
                    var emailKind = (EmailDataKind)typeInt;
                    return emailKind switch
                    {
                        EmailDataKind.Home => ContactType.Personal,
                        EmailDataKind.Work => ContactType.Work,
                        _ => ContactType.Unknown
                    };
                }
                catch (Exception)
                {
                    return ContactType.Unknown;
                }
            }
            return ContactType.Unknown;
        }
    }
}
