using Microsoft.Maui.Controls.Embedding;
using Microsoft.Maui.Platform;

namespace Maui.Controls.Sample;

public partial class MyMauiContent
{
	int count = 0;

	public MyMauiContent()
	{
		InitializeComponent();
	}

	public Image DotNetBot => image;

	private async void OnCounterClicked(object sender, EventArgs e)
	{
		count++;

		if (count == 1)
			CounterBtn.Text = $"Clicked {count} time";
		else
			CounterBtn.Text = $"Clicked {count} times";

		SemanticScreenReader.Announce(CounterBtn.Text);

		await image.ScaleToAsync(1.2, 60);
		await image.ScaleToAsync(1, 60);
	}
}
