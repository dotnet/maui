namespace Maui.Controls.Sample;

public partial class TestPage : ContentPage
{
	public string? Description { get; set; } = "N/A";

	public TestPage()
	{
		BindingContext = this;
		InitializeComponent();
	}

}
