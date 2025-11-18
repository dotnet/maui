using Maui.Controls.Sample.Models;

namespace Maui.Controls.Sample.Views.Landmarks;

public partial class LandmarkListItemView : ContentView
{
    public static readonly BindableProperty LandmarkProperty =
        BindableProperty.Create(nameof(Landmark), typeof(Landmark), typeof(LandmarkListItemView), null);

    public Landmark? Landmark
    {
        get => (Landmark?)GetValue(LandmarkProperty);
        set => SetValue(LandmarkProperty, value);
    }

    public event EventHandler<Landmark>? LandmarkTapped;

    public LandmarkListItemView()
    {
        InitializeComponent();
    }

    private void OnLandmarkTapped(object? sender, TappedEventArgs e)
    {
        if (Landmark is not null)
        {
            LandmarkTapped?.Invoke(this, Landmark);
        }
    }
}
