using System.Threading.Tasks;

namespace Microsoft.Maui.ApplicationModel.Communication
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Email.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Email']/Docs" />
	partial class EmailImplementation : IEmail
	{
		public bool IsComposeSupported =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		Task PlatformComposeAsync(EmailMessage message) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
