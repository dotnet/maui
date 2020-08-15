using System;
using System.Collections.Generic;
using System.Linq;

namespace Xamarin.Essentials
{
    public readonly struct Contact : IEquatable<Contact>
    {
        public string Name { get; }

        public ContactType ContactType { get; }

        public IReadOnlyList<ContactPhone> Numbers { get; }

        public IReadOnlyList<ContactEmail> Emails { get; }

        public DateTime? Birthday { get; }

        internal Contact(
            string name,
            List<ContactPhone> numbers,
            List<ContactEmail> email,
            DateTime? bd,
            ContactType contactType)
        {
            Name = name;
            Birthday = bd;
            Emails = email;
            Numbers = numbers;
            ContactType = contactType;
        }

        public static bool operator ==(Contact left, Contact right) =>
            left.Equals(right);

        public static bool operator !=(Contact left, Contact right) =>
            !left.Equals(right);

        public override bool Equals(object obj) =>
            (obj is Contact contact) && Equals(contact);

        public bool Equals(Contact other) =>
            Name.Equals(other.Name) &&
            Numbers.Equals(other.Numbers) &&
            Emails.Equals(other.Emails) &&
            Birthday.Equals(other.Birthday);

        public override int GetHashCode() =>
            (Name, Numbers, Emails, Birthday).GetHashCode();
    }

    public readonly struct ContactEmail : IEquatable<ContactEmail>
    {
        public string EmailAddress { get; }

        public ContactType ContactType { get; }

        internal ContactEmail(string email, ContactType contactType)
        {
            EmailAddress = email;
            ContactType = contactType;
        }

        public bool Equals(ContactEmail other) =>
            (EmailAddress, ContactType) == (other.EmailAddress, other.ContactType);

        public static bool operator ==(ContactEmail left, ContactEmail right) =>
            left.Equals(right);

        public static bool operator !=(ContactEmail left, ContactEmail right) =>
            !left.Equals(right);

        public override bool Equals(object obj) =>
            (obj is ContactEmail contactEmail) && Equals(contactEmail);

        public override int GetHashCode() =>
            (EmailAddress, ContactType).GetHashCode();
    }

    public readonly struct ContactPhone : IEquatable<ContactPhone>
    {
        public string PhoneNumber { get; }

        public ContactType ContactType { get; }

        internal ContactPhone(string phoneNumber, ContactType contactType)
        {
            PhoneNumber = phoneNumber;
            ContactType = contactType;
        }

        public bool Equals(ContactPhone other) =>
            (PhoneNumber, ContactType) == (other.PhoneNumber, other.ContactType);

        public static bool operator ==(ContactPhone left, ContactPhone right) =>
            left.Equals(right);

        public static bool operator !=(ContactPhone left, ContactPhone right) =>
            !left.Equals(right);

        public override bool Equals(object obj) =>
            (obj is ContactPhone contactPhone) && Equals(contactPhone);

        public override int GetHashCode() =>
            (PhoneNumber, ContactType).GetHashCode();
    }
}
