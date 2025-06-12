using Microsoft.Maui.ManualTests.Categories;

namespace Microsoft.Maui.ManualTests.Tests.RefreshView;

[Test(
	id: "D19",
	title: "Window resize works correctly.",
	category: Category.Editor)]
public partial class D19 : ContentPage
{
	public D19()
	{
		InitializeComponent();
	}

	private void Editor_Loaded(object sender, EventArgs e)
	{
		for (int i = 0; i < 40; i++)
		{
			Editor.Text += "testing\n";
		}
	}
}
