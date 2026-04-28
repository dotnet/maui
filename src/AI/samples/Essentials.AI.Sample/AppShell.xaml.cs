using Maui.Controls.Sample.Pages;

namespace Maui.Controls.Sample;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute(nameof(LandmarkDetailPage), typeof(LandmarkDetailPage));
		Routing.RegisterRoute(nameof(TripPlanningPage), typeof(TripPlanningPage));
	}
}
