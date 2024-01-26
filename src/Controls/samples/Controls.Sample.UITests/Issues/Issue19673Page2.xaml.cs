using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues;

public partial class Issue19673Page2 : ContentPage
{
	public Issue19673Page2()
	{
		InitializeComponent();
	}

	void OnToolbarItemClicked(object sender, System.EventArgs e)
	{
		LabelResult.Text = "Success";
	}
}