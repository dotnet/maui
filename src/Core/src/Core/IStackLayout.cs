namespace Microsoft.Maui
{
	/// <summary>
	/// A Layout that positions child elements in a single line which can be oriented vertically or horizontally.
	/// </summary>
	public interface IStackLayout : ILayout
	{
		/// <summary>
		/// Specifies the amount of space between children.
		/// </summary>
		double Spacing { get; }
	}
}