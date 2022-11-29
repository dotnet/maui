namespace Microsoft.Maui
{
	/// <summary>
	/// Enumerates return button styles.
	/// Typically the operating system on-screen keyboard will visually style the return key based on this value.
	/// </summary>
	public enum ReturnType
	{
		/// <summary>
		/// Indicates the default style on the platform.
		/// </summary>
		Default,

		/// <summary>
		/// Indicates a "Done" button.
		/// </summary>
		Done,

		/// <summary>
		/// Indicates a "Go" button.
		/// </summary>
		Go,
		
		/// <summary>
		/// Indicates a "Next" button. 
		/// </summary>
		Next,

		/// <summary>
		/// Indicates a "Search" button.
		/// </summary>
		Search,
		
		/// <summary>
		/// Indicates a "Send" button.
		/// </summary>
		Send,
	}
}
