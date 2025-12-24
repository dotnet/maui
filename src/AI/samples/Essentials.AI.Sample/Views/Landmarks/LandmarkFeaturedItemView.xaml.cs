using Maui.Controls.Sample.Models;

namespace Maui.Controls.Sample.Views.Landmarks;

public partial class LandmarkFeaturedItemView : ContentView
{
    public static readonly BindableProperty LandmarkProperty =
        BindableProperty.Create(nameof(Landmark), typeof(Landmark), typeof(LandmarkFeaturedItemView), null);

    public Landmark? Landmark
    {
        get => (Landmark?)GetValue(LandmarkProperty);
        set => SetValue(LandmarkProperty, value);
    }

    public event EventHandler<Landmark>? LandmarkTapped;

    public LandmarkFeaturedItemView()
    {
        InitializeComponent();
    }

    private void OnFeaturedLandmarkTapped(object? sender, TappedEventArgs e)
    {
        if (Landmark is not null)
        {
            LandmarkTapped?.Invoke(this, Landmark);
        }
    }
}
