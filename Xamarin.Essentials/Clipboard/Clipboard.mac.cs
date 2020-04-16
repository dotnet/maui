using System.Collections.Generic;
using System.Threading.Tasks;
using AppKit;
using Foundation;

namespace Xamarin.Essentials
{
    public static partial class Clipboard
    {
        static readonly string pasteboardType = NSPasteboard.NSPasteboardTypeString;
        static readonly string[] pasteboardTypes = { pasteboardType };

        static NSPasteboard Pasteboard => NSPasteboard.GeneralPasteboard;

        static Task PlatformSetTextAsync(string text)
        {
            Pasteboard.DeclareTypes(pasteboardTypes, null);
            Pasteboard.ClearContents();
            Pasteboard.SetStringForType(text, pasteboardType);

            return Task.CompletedTask;
        }

        static bool PlatformHasText =>
            Pasteboard.GetStringForType(pasteboardType) != null;

        static Task<string> PlatformGetTextAsync()
        {
            var strs = Pasteboard.ReadObjectsForClasses(
                new ObjCRuntime.Class[] { new ObjCRuntime.Class(typeof(NSString)) },
                null);

            return Task.FromResult(strs?[0]?.ToString());
        }

        static void StartClipboardListeners()
            => throw ExceptionUtils.NotSupportedOrImplementedException;

        static void StopClipboardListeners()
            => throw ExceptionUtils.NotSupportedOrImplementedException;
    }
}
