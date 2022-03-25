using System;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Microsoft.Maui.ApplicationModel.DataTransfer
{
	public interface IClipboard
	{
		bool HasText { get; }

		Task SetTextAsync(string text);

		Task<string> GetTextAsync();

		void StartClipboardListeners();

		void StopClipboardListeners();
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/Clipboard.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Clipboard']/Docs" />
	public static partial class Clipboard
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Clipboard.xml" path="//Member[@MemberName='SetTextAsync']/Docs" />
		public static Task SetTextAsync(string text)
			=> Current.SetTextAsync(text ?? string.Empty);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Clipboard.xml" path="//Member[@MemberName='HasText']/Docs" />
		public static bool HasText
			=> Current.HasText;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Clipboard.xml" path="//Member[@MemberName='GetTextAsync']/Docs" />
		public static Task<string> GetTextAsync()
			=> Current.GetTextAsync();

		public static event EventHandler<EventArgs> ClipboardContentChanged
		{
			add
			{
				var wasRunning = ClipboardContentChangedInternal != null;

				ClipboardContentChangedInternal += value;

				if (!wasRunning && ClipboardContentChangedInternal != null)
				{
					Current.StartClipboardListeners();
				}
			}

			remove
			{
				var wasRunning = ClipboardContentChangedInternal != null;

				ClipboardContentChangedInternal -= value;

				if (wasRunning && ClipboardContentChangedInternal == null)
					Current.StopClipboardListeners();
			}
		}

		static event EventHandler<EventArgs> ClipboardContentChangedInternal;

		internal static void ClipboardChangedInternal() => ClipboardContentChangedInternal?.Invoke(null, EventArgs.Empty);

#nullable enable
		static IClipboard? currentImplementation;
#nullable disable

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IClipboard Current =>
			currentImplementation ??= new ClipboardImplementation();

		[EditorBrowsable(EditorBrowsableState.Never)]
#nullable enable
		public static void SetCurrent(IClipboard? implementation) =>
			currentImplementation = implementation;
#nullable disable
	}
}
