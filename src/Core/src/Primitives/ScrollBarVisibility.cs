namespace Microsoft.Maui
{
	public enum ScrollBarVisibility
	{
		/// <summary>
		/// The visibility of the scrollbar will be the default for the platform based on the content and orientation.
		/// </summary>
		Default = 0,

		/// <summary>
		/// The scollbar will be visible, regardless of the content or orientation.
		/// </summary>
		Always,

		/// <summary>
		/// The scrollbar will not be visible, regardless of the content or orientation.
		/// </summary>
		Never,
	}
}