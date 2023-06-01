namespace Microsoft.Maui
{
	/// <summary>
	/// Indicates a control which supports cross-platform layout operations
	/// </summary>
	public interface ICrossPlatformLayoutBacking
	{
		/// <summary>
		/// Gets or sets the implementation of cross-platform layout operations to be carried out by this control
		/// </summary>
		/// <remarks>
		/// This property is the bridge between the platform-level backing control and the cross-platform-level
		/// layout. It is typically connected by the handler, which may add additional logic to normalize layout
		/// and content behaviors across the various platforms. 
		/// </remarks>
		public ICrossPlatformLayout? CrossPlatformLayout { get; set; }
	}
}
