using System.Collections.Generic;
using System.Threading.Tasks;
using AppKit;
using Foundation;

namespace Microsoft.Maui.ApplicationModel.DataTransfer
{
	partial class ClipboardImplementation : IClipboard
	{
		readonly string pasteboardType = NSPasteboard.NSPasteboardTypeString;
		readonly string[] pasteboardTypes = { pasteboardType };

		NSPasteboard Pasteboard => NSPasteboard.GeneralPasteboard;

		public Task SetTextAsync(string text)
		{
			Pasteboard.DeclareTypes(pasteboardTypes, null);
			Pasteboard.ClearContents();
			Pasteboard.SetStringForType(text, pasteboardType);

			return Task.CompletedTask;
		}

		public bool HasText =>
			!string.IsNullOrEmpty(GetPasteboardText());

		public Task<string> GetTextAsync()
			=> Task.FromResult(GetPasteboardText());

		string GetPasteboardText()
			=> Pasteboard.ReadObjectsForClasses(
				new ObjCRuntime.Class[] { new ObjCRuntime.Class(typeof(NSString)) },
				null)?[0]?.ToString();

		void StartClipboardListeners()
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		void StopClipboardListeners()
			=> throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
