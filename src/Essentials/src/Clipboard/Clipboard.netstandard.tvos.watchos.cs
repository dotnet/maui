#nullable enable
using System.Threading.Tasks;

namespace Microsoft.Maui.ApplicationModel.DataTransfer
{
	partial class ClipboardImplementation : IClipboard
	{
		public Task SetTextAsync(string? text)
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		public bool HasText
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		public Task<string?> GetTextAsync()
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		void StartClipboardListeners()
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		void StopClipboardListeners()
			=> throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
