using System.Collections.Generic;


namespace Microsoft.Maui
{
	/// <summary>
	/// A Layout that positions child elements in a single line which can be oriented vertically or horizontally.
	/// </summary>
	public interface IStackLayout : ILayout
	{
		/// <summary>
		/// Identifies the Spacing between childs.
		/// </summary>
		int Spacing { get; }
	}
}