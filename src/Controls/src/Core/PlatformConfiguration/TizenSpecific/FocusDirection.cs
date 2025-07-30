#nullable disable
using System.ComponentModel;

namespace Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific
{
	/// <summary>Contains constants for describing focus directions.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class FocusDirection
	{
		/// <summary>The constant for specifying no focus direction.</summary>
		public const string None = "None";
		/// <summary>The constant for specifying the back focus direction.</summary>
		public const string Back = "Back";
		/// <summary>The constant for specifying the forward focus direction.</summary>
		public const string Forward = "Forward";
		/// <summary>The constant for specifying the up focus direction.</summary>
		public const string Up = "Up";
		/// <summary>The constant for specifying the down focus direction.</summary>
		public const string Down = "Down";
		/// <summary>The constant for specifying the right focus direction.</summary>
		public const string Right = "Right";
		/// <summary>The constant for specifying the left focus direction.</summary>
		public const string Left = "Left";
	}
}