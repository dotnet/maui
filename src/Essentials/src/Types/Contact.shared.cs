using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Maui.ApplicationModel.Communication
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Contact.xml" path="Type[@FullName='Microsoft.Maui.ApplicationModel.Communication.Contact']/Docs/*" />
	public class Contact
	{
		string displayName;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Contact.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public Contact()
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/Contact.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public Contact(
			string id,
			string namePrefix,
			string givenName,
			string middleName,
			string familyName,
			string nameSuffix,
			IEnumerable<ContactPhone> phones,
			IEnumerable<ContactEmail> email,
			string displayName = null)
		{
			Id = id;
			NamePrefix = namePrefix;
			GivenName = givenName;
			MiddleName = middleName;
			FamilyName = familyName;
			NameSuffix = nameSuffix;
			Phones.AddRange(phones?.ToList());
			Emails.AddRange(email?.ToList());
			DisplayName = displayName;
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/Contact.xml" path="//Member[@MemberName='Id']/Docs/*" />
		public string Id { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/Contact.xml" path="//Member[@MemberName='DisplayName']/Docs/*" />
		public string DisplayName
		{
			get => !string.IsNullOrWhiteSpace(displayName) ? displayName : BuildDisplayName();
			private set => displayName = value;
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/Contact.xml" path="//Member[@MemberName='NamePrefix']/Docs/*" />
		public string NamePrefix { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/Contact.xml" path="//Member[@MemberName='GivenName']/Docs/*" />
		public string GivenName { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/Contact.xml" path="//Member[@MemberName='MiddleName']/Docs/*" />
		public string MiddleName { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/Contact.xml" path="//Member[@MemberName='FamilyName']/Docs/*" />
		public string FamilyName { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/Contact.xml" path="//Member[@MemberName='NameSuffix']/Docs/*" />
		public string NameSuffix { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/Contact.xml" path="//Member[@MemberName='Phones']/Docs/*" />
		public List<ContactPhone> Phones { get; set; } = new List<ContactPhone>();

		/// <include file="../../docs/Microsoft.Maui.Essentials/Contact.xml" path="//Member[@MemberName='Emails']/Docs/*" />
		public List<ContactEmail> Emails { get; set; } = new List<ContactEmail>();

		/// <include file="../../docs/Microsoft.Maui.Essentials/Contact.xml" path="//Member[@MemberName='ToString']/Docs/*" />
		public override string ToString() => DisplayName;

		string BuildDisplayName()
		{
			if (string.IsNullOrWhiteSpace(GivenName))
				return FamilyName;
			if (string.IsNullOrWhiteSpace(FamilyName))
				return GivenName;

			return $"{GivenName} {FamilyName}";
		}
	}

	public class ContactEmail
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/ContactEmail.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public ContactEmail()
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/ContactEmail.xml" path="//Member[@MemberName='.ctor'][2]/Docs/*" />
		public ContactEmail(string emailAddress)
		{
			EmailAddress = emailAddress;
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/ContactEmail.xml" path="//Member[@MemberName='EmailAddress']/Docs/*" />
		public string EmailAddress { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/ContactEmail.xml" path="//Member[@MemberName='ToString']/Docs/*" />
		public override string ToString() => EmailAddress;
	}

	public class ContactPhone
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/ContactPhone.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public ContactPhone()
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/ContactPhone.xml" path="//Member[@MemberName='.ctor'][2]/Docs/*" />
		public ContactPhone(string phoneNumber)
		{
			PhoneNumber = phoneNumber;
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/ContactPhone.xml" path="//Member[@MemberName='PhoneNumber']/Docs/*" />
		public string PhoneNumber { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/ContactPhone.xml" path="//Member[@MemberName='ToString']/Docs/*" />
		public override string ToString() => PhoneNumber;
	}
}
