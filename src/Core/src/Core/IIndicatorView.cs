using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// A view that displays indicators that represent the number of items, and current position
	/// </summary>
	public interface IIndicatorView : IView
	{
		/// <summary>
		/// The number of indicators
		/// </summary>
		int Count { get; }

		/// <summary>
		/// The currently selected indicator index
		/// </summary>
		int Position { get; set; }

		/// <summary>
		/// Size of the indicators
		/// </summary>
		double IndicatorSize { get; }

		/// <summary>
		/// Maximum number of visible indicators
		/// </summary>
		int MaximumVisible { get; }

		/// <summary>
		/// Indicates whether the indicator should be hidden when only one exists.
		/// </summary>
		bool HideSingle { get; }

		/// <summary>
		/// Color of the indicators
		/// We only support SolidPaint color for now 
		/// </summary>
		Paint? IndicatorColor { get; }

		/// <summary>
		/// Color of the indicator that represents the currently selected index
		/// We only support SolidPaint color for now 
		/// </summary>
		Paint? SelectedIndicatorColor { get; }

		/// <summary>
		/// Shape of platform indicators, can be Circle or Square
		/// </summary>
		IShape IndicatorsShape { get; }
	}

	/// <summary>
	/// A layout that displays indicators that represent the number of items using a DataTemplate for indicators
	/// </summary>
	public interface ITemplatedIndicatorView : IIndicatorView
	{
		/// <summary>
		/// The layout where to renderer the Template for the indicators
		/// </summary>
		ILayout? IndicatorsLayoutOverride { get; }
	}
}
