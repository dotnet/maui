#nullable enable

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Specifies how a <see cref="BottomSheetDetent"/> determines its height.
	/// </summary>
	public enum BottomSheetDetentKind
	{
		/// <summary>
		/// The platform's medium detent: about half of the available height.
		/// On iOS this is the system medium detent, which the system deactivates in compact height (iPhone landscape).
		/// </summary>
		Medium,

		/// <summary>
		/// The platform's large detent: the sheet is fully expanded.
		/// </summary>
		Large,

		/// <summary>
		/// The height the sheet's content needs, limited to the available height.
		/// </summary>
		FitContent,

		/// <summary>
		/// A fraction of the available height, given by <see cref="BottomSheetDetent.Value"/>.
		/// </summary>
		Fraction,

		/// <summary>
		/// A fixed height in device-independent units, given by <see cref="BottomSheetDetent.Value"/>, limited to the available height.
		/// </summary>
		Height,
	}
}
