using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Email.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Email']/Docs" />
	public static partial class Email
	{
		internal static bool IsComposeSupported =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		static Task PlatformComposeAsync(EmailMessage message) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}

#if NETSTANDARD || NET6_0
	/// <include file="../../docs/Microsoft.Maui.Essentials/EmailAttachment.xml" path="Type[@FullName='Microsoft.Maui.Essentials.EmailAttachment']/Docs" />
	public partial class EmailAttachment
	{
		string PlatformGetContentType(string extension) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
#endif
}
