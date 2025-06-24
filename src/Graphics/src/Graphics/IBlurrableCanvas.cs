namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Defines a canvas that supports blur effects.
	/// </summary>
	public interface IBlurrableCanvas
	{
		/// <summary>
		/// Sets the blur radius for subsequent drawing operations.
		/// </summary>
		/// <param name="blurRadius">The radius of the blur effect. A value of 0 indicates no blur.</param>
		void SetBlur(float blurRadius);
	}
}
