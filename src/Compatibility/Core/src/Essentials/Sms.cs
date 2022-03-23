#nullable enable
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel.Communication;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Sms.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Sms']/Docs" />
	public static class Sms
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Sms.xml" path="//Member[@MemberName='ComposeAsync'][1]/Docs" />
		public static Task ComposeAsync()
			=> Current.ComposeAsync(null);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Sms.xml" path="//Member[@MemberName='ComposeAsync'][2]/Docs" />
		public static Task ComposeAsync(SmsMessage? message)
			=> Current.ComposeAsync(message);

		static ISms Current => ApplicationModel.Communication.Sms.Default;
	}
}
