using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public interface IIndicatorView : IView
	{
		int Count { get; }

		int Position { get; set; }

		double IndicatorSize { get; }

		int MaximumVisible { get; }

		bool HideSingle { get; }

		Paint? IndicatorColor { get; }

		Paint? SelectedIndicatorColor { get; }

		IShape IndicatorsShape { get; }
	}

	public interface ITemplatedIndicatorView : IIndicatorView
	{
		ILayout? IndicatorsLayoutOverride { get; }
	}
}
