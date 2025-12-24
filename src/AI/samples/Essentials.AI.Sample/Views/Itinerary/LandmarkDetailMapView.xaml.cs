using Maui.Controls.Sample.Models;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace Maui.Controls.Sample.Views.Itinerary;

public partial class LandmarkDetailMapView : ContentView
{
    public LandmarkDetailMapView()
    {
        InitializeComponent();
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        
        if (BindingContext is Landmark landmark)
        {
            // Set ItemsSource to a single-item collection for the pin
            map.ItemsSource = new[] { landmark };
            
            // Move map to show the landmark
            var center = new Location(landmark.Latitude, landmark.Longitude);
            var span = new MapSpan(center, landmark.Span, landmark.Span);
            
            map.MoveToRegion(span);
        }
    }
}
