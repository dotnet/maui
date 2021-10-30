namespace Microsoft.Maui
{
	/// <summary>
	/// Provides functionality to be able to apply transformations to a View.
	/// </summary>
	public interface ITransform
	{
		/// <summary>
		/// Gets the X translation delta of the element.
		/// </summary>
		double TranslationX { get; }

		/// <summary>
		/// Gets the Y translation delta of the element.
		/// </summary>
		double TranslationY { get; }

		/// <summary>
		/// Gets the scale factor applied to the element.
		/// </summary>
		double Scale { get; }

		/// <summary>
		/// Gets the scale about the X-axis factor applied to the element.
		/// </summary>
		double ScaleX { get; }

		/// <summary>
		/// Gets the scale about the Y-axis factor applied to the element.
		/// </summary>
		double ScaleY { get; }

		/// <summary>
		/// Gets the rotation (in degrees) about the Z-axis (affine rotation)
		/// when the element is rendered.
		/// </summary>
		double Rotation { get; }

		/// <summary>
		/// Gets the rotation (in degrees) about the X-axis (perspective rotation)
		/// when the element is rendered.
		/// </summary>
		double RotationX { get; }

		/// <summary>
		/// Gets the rotation (in degrees) about the Y-axis (perspective rotation)
		/// when the element is rendered.
		/// </summary>
		double RotationY { get; }

		/// <summary>
		/// Gets the X component of the center point for any transform, relative
		/// to the bounds of the element.
		/// </summary>
		double AnchorX { get; }

		/// <summary>
		/// Gets the Y component of the center point for any transform, relative
		/// to the bounds of the element.
		/// </summary>
		double AnchorY { get; }
	}
}