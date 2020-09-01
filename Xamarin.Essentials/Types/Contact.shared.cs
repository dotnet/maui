using System;
using System.Collections.Generic;
using System.Linq;

namespace Xamarin.Essentials
{
    public class Contact
    {
        public string Name { get; }

        public ContactType ContactType { get; }

        public IReadOnlyList<ContactPhone> Numbers { get; }

        public IReadOnlyList<ContactEmail> Emails { get; }

        internal Contact(
            string name,
            List<ContactPhone> numbers,
            List<ContactEmail> email,
            ContactType contactType)
        {
            Name = name;
            Emails = email;
            Numbers = numbers;
            ContactType = contactType;
        }
    }

    public class ContactEmail
    {
        public string EmailAddress { get; }

        public ContactType ContactType { get; }

        internal ContactEmail(string email, ContactType contactType)
        {
            EmailAddress = email;
            ContactType = contactType;
        }
    }

    public class ContactPhone
    {
        public string PhoneNumber { get; }

        public ContactType ContactType { get; }

        internal ContactPhone(string phoneNumber, ContactType contactType)
        {
            PhoneNumber = phoneNumber;
            ContactType = contactType;
        }
    }
}
