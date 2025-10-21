using Microsoft.Maui.ManualTests.Categories;

namespace Microsoft.Maui.ManualTests.Tests.BugFixes;

[Test(
	id: "L1",
	title: "TapGestureRecognizer",
	category: Category.BugFixes)]
public partial class L1_TapGestureRecognizer : ContentPage
{
	int count = 0;
	public L1_TapGestureRecognizer()
	{
		InitializeComponent();
	}

	private void OnCounterClicked(object sender, EventArgs e)
	{
		count++;

		if (count == 1)
			CounterLabel.Text = $"Clicked {count} time";
		else
			CounterLabel.Text = $"Clicked {count} times";

		SemanticScreenReader.Announce(CounterLabel.Text);
	}
}