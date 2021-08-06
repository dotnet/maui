using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public interface IIndicatorView : IView
	{

		int Count { get; }

		int Position { get; }

		double IndicatorSize { get; }

		int MaximumVisible { get; }

		bool HideSingle { get; }

		Paint IndicatorsColor { get; }

		Paint PositionIndicatorColor { get;  }
	}
}
