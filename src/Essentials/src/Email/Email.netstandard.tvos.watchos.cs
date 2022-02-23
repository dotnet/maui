using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials.Implementations
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Email.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Email']/Docs" />
	public partial class EmailImplementation : IEmail
	{
		public bool IsComposeSupported =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public Task ComposeAsync(EmailMessage message) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public Task ComposeAsync(string subject, string body, params string[] to) 
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		public Task ComposeAsync() =>
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
