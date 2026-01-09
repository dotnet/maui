namespace Maui.Controls.Sample;

public class ClipControlPage : NavigationPage
{
    private ClipViewModel _viewModel;
    public ClipControlPage()
    {
        _viewModel = new ClipViewModel();
        PushAsync(new ClipControlMainPage(_viewModel));
    }
}

public partial class ClipControlMainPage : ContentPage
{
    private ClipViewModel _viewModel;
    public ClipControlMainPage(ClipViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
    {
        BindingContext = _viewModel = new ClipViewModel();
        await Navigation.PushAsync(new ClipOptionsPage(_viewModel));
    }

    private void ControlType_Clicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            UpdateCurrentControl(button.Text);
        }
    }

    private void UpdateCurrentControl(string controlType)
    {
        // Clear the container completely - like SearchBar example: SearchBarGrid.Children.Clear();
        CurrentControlContainer.Clear();

        View newControl = controlType switch
        {
            "Image" => CreateImageControl(),
            "Label" => CreateLabelControl(),
            "Button" => CreateButtonControl(),
            "Border" => CreateBorderControl(),
            "BoxView" => CreateBoxViewControl(),
            _ => new Label { Text = "Unknown Control" }
        };

        // Add the new control - like SearchBar example: SearchBarGrid.Children.Add(searchBar);
        CurrentControlContainer.Children.Add(newControl);
    }

    private Image CreateImageControl()
    {
        var image = new Image
        {
            Source = "blue.png",
            WidthRequest = 300,
            HeightRequest = 300,
            Aspect = Aspect.AspectFill,
            AutomationId = "TestControl"
        };
        image.SetBinding(VisualElement.ClipProperty, new Binding(nameof(ClipViewModel.Clip)));
        return image;
    }

    private Label CreateLabelControl()
    {
        var label = new Label
        {
            Text = "Sample Text for Clipping Test\nMultiple Lines\nTo Show Clip Effect",
            FontSize = 28,
            FontAttributes = FontAttributes.Bold,
            BackgroundColor = Colors.LightBlue,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
            WidthRequest = 300,
            HeightRequest = 300,
            AutomationId = "TestControl"
        };
        label.SetBinding(VisualElement.ClipProperty, new Binding(nameof(ClipViewModel.Clip)));
        return label;
    }

    private Button CreateButtonControl()
    {
        var button = new Button
        {
            Text = "Test Button\nWith Clip",
            FontSize = 20,
            BackgroundColor = Colors.Orange,
            TextColor = Colors.White,
            WidthRequest = 300,
            HeightRequest = 300,
            AutomationId = "TestControl"
        };
        button.SetBinding(VisualElement.ClipProperty, new Binding(nameof(ClipViewModel.Clip)));
        return button;
    }

    private Border CreateBorderControl()
    {
        var border = new Border
        {
            BackgroundColor = Colors.Purple,
            Stroke = Colors.DarkMagenta,
            StrokeThickness = 5,
            WidthRequest = 300,
            HeightRequest = 300,
            AutomationId = "TestControl",
            Content = new Label
            {
                Text = "Border Content\nWith Clipping",
                TextColor = Colors.White,
                FontSize = 24,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            }
        };
        border.SetBinding(VisualElement.ClipProperty, new Binding(nameof(ClipViewModel.Clip)));
        return border;
    }

    private BoxView CreateBoxViewControl()
    {
        var boxView = new BoxView
        {
            Color = Colors.Red,
            WidthRequest = 300,
            HeightRequest = 300,
            AutomationId = "TestControl"
        };
        boxView.SetBinding(VisualElement.ClipProperty, new Binding(nameof(ClipViewModel.Clip)));
        return boxView;
    }
}