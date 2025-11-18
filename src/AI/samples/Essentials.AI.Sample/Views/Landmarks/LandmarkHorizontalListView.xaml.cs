using System.Collections;
using Maui.Controls.Sample.Models;

namespace Maui.Controls.Sample.Views.Landmarks;

public partial class LandmarkHorizontalListView : ContentView
{
    public static readonly BindableProperty LandmarksProperty =
        BindableProperty.Create(nameof(Landmarks), typeof(IEnumerable), typeof(LandmarkHorizontalListView), null);

    public IEnumerable? Landmarks
    {
        get => (IEnumerable?)GetValue(LandmarksProperty);
        set => SetValue(LandmarksProperty, value);
    }

    public event EventHandler<Landmark>? LandmarkTapped;

    public LandmarkHorizontalListView()
    {
        InitializeComponent();
    }

    private void OnLandmarkItemTapped(object? sender, Landmark landmark)
    {
        LandmarkTapped?.Invoke(this, landmark);
    }
}
