using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials.Implementations
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Clipboard.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Clipboard']/Docs" />
	public partial class ClipboardImplementation : IClipboard
	{
		public Task SetTextAsync(string text)
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		public bool HasText
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		public Task<string> GetTextAsync()
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		public void StartClipboardListeners()
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		public void StopClipboardListeners()
			=> throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
