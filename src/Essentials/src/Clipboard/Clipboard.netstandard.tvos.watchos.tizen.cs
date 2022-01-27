using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Clipboard.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Clipboard']/Docs" />
	public static partial class Clipboard
	{
		static Task PlatformSetTextAsync(string text)
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		static bool PlatformHasText
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		static Task<string> PlatformGetTextAsync()
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		static void StartClipboardListeners()
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		static void StopClipboardListeners()
			=> throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
