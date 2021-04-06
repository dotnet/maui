namespace Microsoft.Maui
{
	/// <summary>
	///  Represents a View that consists of two buttons labeled with minus and plus signs.
	///  Use a Stepper for selecting a numeric value from a range of values.
	/// </summary>
	public interface IStepper : IView, IRange
	{
		/// <summary>
		/// Gets the amount by which Value is increased or decreased.
		/// </summary>
		double? Interval { get; }
	}
}