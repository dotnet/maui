namespace Maui.Controls.Sample;

public class ClipControlPage : NavigationPage
{
    public ClipControlPage()
    {
        PushAsync(new ClipControlMainPage());
    }
}

public partial class ClipControlMainPage : ContentPage
{
    public ClipControlMainPage()
    {
        InitializeComponent();
    }

    private void OnBorderPageClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new BorderClip());
    }

    private void OnBoxViewPageClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new BoxViewClip());
    }

    private void OnButtonPageClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new ButtonClip());
    }

    private void OnImagePageClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new ImageClip());
    }

    private void OnLabelPageClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new LabelClip());
    }

    private void OnContentViewPageClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new ContentViewClip());
    }

    private void OnImageButtonPageClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new ImageButtonClip());
    }
}