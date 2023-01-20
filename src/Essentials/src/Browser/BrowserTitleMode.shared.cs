#nullable enable
namespace Microsoft.Maui.ApplicationModel
{
	/// <summary>
	/// Mode for the in-app browser title.
	/// </summary>
	/// <remarks>These values only apply to Android.</remarks>
	public enum BrowserTitleMode
	{
		/// <summary>Uses the system default.</summary>
		Default = 0,

		/// <summary>Show the title.</summary>
		Show = 1,

		/// <summary>Hide the title.</summary>
		Hide = 2
	}
}
