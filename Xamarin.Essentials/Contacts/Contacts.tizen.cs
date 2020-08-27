using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tizen.Applications;
using Tizen.Pims.Contacts;
using TizenContact = Tizen.Pims.Contacts.ContactsViews.Contact;
using TizenEmail = Tizen.Pims.Contacts.ContactsViews.Email;
using TizenName = Tizen.Pims.Contacts.ContactsViews.Name;
using TizenNumber = Tizen.Pims.Contacts.ContactsViews.Number;

namespace Xamarin.Essentials
{
    public static partial class Contacts
    {
        static async Task<Contact> PlatformPickContactAsync()
        {
            Permissions.EnsureDeclared<Permissions.ContactsRead>();
            await Permissions.EnsureGrantedAsync<Permissions.ContactsRead>();

            var tcs = new TaskCompletionSource<Contact>();

            var appControl = new AppControl();
            appControl.Operation = AppControlOperations.Pick;
            appControl.ExtraData.Add(AppControlData.SectionMode, "single");
            appControl.LaunchMode = AppControlLaunchMode.Single;
            appControl.Mime = "application/vnd.tizen.contact";

            AppControl.SendLaunchRequest(appControl, (request, reply, result) =>
            {
                Contact contact = null;

                if (result == AppControlReplyResult.Succeeded)
                {
                    var contactId = reply.ExtraData.Get<IEnumerable<string>>(AppControlData.Selected)?.FirstOrDefault();

                    if (int.TryParse(contactId, out var contactInt))
                    {
                        var mgr = new ContactsManager();

                        var record = mgr.Database.Get(TizenContact.Uri, contactInt);

                        if (record != null)
                        {
                            string name = null;
                            var emails = new List<ContactEmail>();
                            var phones = new List<ContactPhone>();

                            var recordName = record.GetChildRecord(TizenContact.Name, 0);
                            if (recordName != null)
                            {
                                var first = recordName.Get<string>(TizenName.First) ?? string.Empty;
                                var last = recordName.Get<string>(TizenName.Last) ?? string.Empty;

                                name = $"{first} {last}".Trim();
                            }

                            var emailCount = record.GetChildRecordCount(TizenContact.Email);
                            for (var i = 0; i < emailCount; i++)
                            {
                                var item = record.GetChildRecord(TizenContact.Email, i);
                                var addr = item.Get<string>(TizenEmail.Address);
                                var type = (TizenEmail.Types)item.Get<int>(TizenEmail.Type);

                                emails.Add(new ContactEmail(addr, GetContactType(type)));
                            }

                            var phoneCount = record.GetChildRecordCount(TizenContact.Number);
                            for (var i = 0; i < phoneCount; i++)
                            {
                                var item = record.GetChildRecord(TizenContact.Number, i);
                                var number = item.Get<string>(TizenNumber.NumberData);
                                var type = (TizenNumber.Types)item.Get<int>(TizenNumber.Type);

                                phones.Add(new ContactPhone(number, GetContactType(type)));
                            }

                            contact = new Contact(name, phones, emails, ContactType.Unknown);
                        }
                    }
                }

                tcs.TrySetResult(contact);
            });

            return await tcs.Task;
        }

        static ContactType GetContactType(TizenEmail.Types emailType)
            => emailType switch
            {
                TizenEmail.Types.Home => ContactType.Personal,
                TizenEmail.Types.Mobile => ContactType.Personal,
                TizenEmail.Types.Work => ContactType.Work,
                _ => ContactType.Unknown
            };

        static ContactType GetContactType(TizenNumber.Types numberType)
            => numberType switch
            {
                TizenNumber.Types.Car => ContactType.Personal,
                TizenNumber.Types.Cell => ContactType.Personal,
                TizenNumber.Types.Home => ContactType.Personal,
                TizenNumber.Types.Main => ContactType.Personal,
                TizenNumber.Types.Message => ContactType.Personal,
                TizenNumber.Types.Video => ContactType.Personal,
                TizenNumber.Types.Voice => ContactType.Personal,
                TizenNumber.Types.Work => ContactType.Work,
                TizenNumber.Types.Pager => ContactType.Work,
                TizenNumber.Types.Assistant => ContactType.Work,
                TizenNumber.Types.Company => ContactType.Work,
                TizenNumber.Types.Fax => ContactType.Work,
                _ => ContactType.Unknown
            };
    }
}
