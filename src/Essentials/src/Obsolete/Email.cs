#nullable enable
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel.Communication;

namespace Microsoft.Maui.ApplicationModel.Communication
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Email.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Email']/Docs" />
	public static class Email
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Email.xml" path="//Member[@MemberName='ComposeAsync'][1]/Docs" />
		public static Task ComposeAsync() =>
			Current.ComposeAsync();

		/// <include file="../../docs/Microsoft.Maui.Essentials/Email.xml" path="//Member[@MemberName='ComposeAsync'][3]/Docs" />
		public static Task ComposeAsync(string subject, string body, params string[] to) =>
			Current.ComposeAsync(subject, body, to);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Email.xml" path="//Member[@MemberName='ComposeAsync'][2]/Docs" />
		public static Task ComposeAsync(EmailMessage message) =>
			Current.ComposeAsync(message);

		static IEmail Current => ApplicationModel.Communication.Email.Default;
	}
}
