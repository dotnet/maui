namespace Microsoft.Maui.Controls
{
	/// <summary>Specifies scrolling behavior for a <see cref="ScrollView"/>.</summary>
	public enum ScrollMode
	{
		/// <summary>Scrolling is disabled in this direction.</summary>
		Disabled = 0,
		/// <summary>Scrolling is always enabled in this direction.</summary>
		Enabled = 1,
		/// <summary>Scrolling is enabled only when content exceeds the viewport.</summary>
		Auto = 2
	}
}
