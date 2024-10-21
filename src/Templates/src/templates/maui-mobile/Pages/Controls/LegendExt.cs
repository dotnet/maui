using Syncfusion.Maui.Toolkit.Charts;

namespace MauiApp._1.Pages.Controls;

public class LegendExt : ChartLegend
{
	protected override double GetMaximumSizeCoefficient()
	{
		return 0.5;
	}
}
