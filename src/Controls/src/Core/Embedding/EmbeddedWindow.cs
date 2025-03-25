#if ANDROID || IOS || MACCATALYST || WINDOWS

namespace Microsoft.Maui.Controls.Embedding;

/// <summary>
/// An embedded window that is never used directly, but instead forms a connection between
/// the MAUI app and the embedded MAUI content.
/// </summary>
class EmbeddedWindow : Window, IWindow
{
	/// <summary>
	/// Always returns null because this window will never have any direct content.
	/// </summary>
	IView IWindow.Content => null!;
}

#endif
