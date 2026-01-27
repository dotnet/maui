namespace Microsoft.Maui.Controls
{
	/// <summary>Specifies how a constraint is defined.</summary>
	public enum ConstraintType
	{
		/// <summary>Constraint is relative to the parent element.</summary>
		RelativeToParent,
		/// <summary>Constraint is relative to another view.</summary>
		RelativeToView,
		/// <summary>Constraint is a constant value.</summary>
		Constant
	}
}