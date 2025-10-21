using System;
namespace Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific
{
	/// <summary>Enumerates preferences for displaying large titles.</summary>
	public enum LargeTitleDisplayMode
	{
		/// <summary>Display large titles if the previous screen had displayed large titles.</summary>
		Automatic,
		/// <summary>Always display large titles.</summary>
		Always,
		/// <summary>Never display large titles.</summary>
		Never
	}
}
