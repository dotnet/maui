namespace Microsoft.Maui
{
	/// <summary>
	/// Exposes the state of an element handler.
	/// </summary>
	/// <remarks>
	/// To be migrated to a public API.
	/// </remarks>
	internal interface IElementHandlerStateExhibitor
	{
		/// <summary>  
		/// Gets the state of the element handler.  
		/// </summary>  
		ElementHandlerState State { get; }
	}
}