using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Contacts;
using ContactsUI;

namespace Xamarin.Essentials
{
    public static partial class Contacts
    {
        internal static Action<Contact?> CallBack { get; set; }

        internal static Action<Exception> ErrorCallBack { get; set; }

        static Task<Contact?> PlatformPickContactAsync()
        {
            var uiView = Platform.GetCurrentViewController();
            if (uiView == null)
                throw new ArgumentNullException($"The View Controller can't be null.");

            using var picker = new CNContactPickerViewController
            {
                Delegate = new ContactPickerDelegate()
            };

            uiView.PresentViewController(picker, true, null);
            var source = new TaskCompletionSource<Contact?>();
            try
            {
                CallBack = (phoneContact) =>
                {
                    var tcs = Interlocked.Exchange(ref source, null);
                    tcs?.SetResult(phoneContact);
                };

                ErrorCallBack = (ex) =>
                {
                    var tcs = Interlocked.Exchange(ref source, null);
                    tcs?.SetException(ex);
                };
            }
            catch (Exception ex)
            {
                source.SetException(ex);
            }
            return source.Task;
        }

        internal static Contact GetContact(CNContact contact)
        {
            if (contact == null)
                return default;

            try
            {
                var contactType = ToPhoneContact(contact.ContactType);
                var phones = new List<ContactPhone>();

                foreach (var item in contact.PhoneNumbers)
                    phones.Add(new ContactPhone(item?.Value?.StringValue, contactType));

                var emails = new List<ContactEmail>();

                foreach (var item in contact.EmailAddresses)
                    emails.Add(new ContactEmail(item?.Value?.ToString(), contactType));

                var name = string.Empty;

                if (string.IsNullOrEmpty(contact.MiddleName))
                    name = $"{contact.GivenName} {contact.FamilyName}";
                else
                    name = $"{contact.GivenName} {contact.MiddleName} {contact.FamilyName}";

                var birthday = contact.Birthday?.Date.ToDateTime().Date;
                return new Contact(
                                    name,
                                    phones,
                                    emails,
                                    birthday,
                                    contactType);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                contact.Dispose();
            }
        }

        static ContactType ToPhoneContact(CNContactType type) => type switch
        {
            CNContactType.Person => ContactType.Personal,
            CNContactType.Organization => ContactType.Work,
            _ => ContactType.Unknown,
        };
    }

    public class ContactPickerDelegate : CNContactPickerDelegate
    {
        public ContactPickerDelegate()
        {
        }

        public ContactPickerDelegate(IntPtr handle)
            : base(handle)
        {
        }

        public override void ContactPickerDidCancel(CNContactPickerViewController picker)
        {
            Contacts.CallBack(default);
            picker.DismissModalViewController(true);
        }

        public override void DidSelectContact(CNContactPickerViewController picker, CNContact contact)
        {
            Contacts.CallBack(Contacts.GetContact(contact));
            picker.DismissModalViewController(true);
        }

        public override void DidSelectContactProperty(CNContactPickerViewController picker, CNContactProperty contactProperty) =>
            picker.DismissModalViewController(true);
    }
}
