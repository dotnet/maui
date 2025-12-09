using Microsoft.Maui.ManualTests.Categories;

namespace Microsoft.Maui.ManualTests.Tests.RefreshView;

[Test(
	id: "D15",
	title: "Events work correctly.",
	category: Category.Editor)]
public partial class D15 : ContentPage
{
	public D15()
	{
		InitializeComponent();
	}

	private void Editor_Loaded(object sender, EventArgs e)
	{
		Label1.BackgroundColor = Colors.LightBlue;
		Label1.TextColor = Colors.Black;
	}

	private void Editor_Focused(object sender, FocusEventArgs e)
	{
		Label2.BackgroundColor = Colors.LightBlue;
		Label2.TextColor = Colors.Black;
	}

	private void Editor_TextChanged(object sender, TextChangedEventArgs e)
	{
		Label3.BackgroundColor = Colors.LightBlue;
		Label3.TextColor = Colors.Black;
	}

	private void Editor_Completed(object sender, EventArgs e)
	{
		Label4.BackgroundColor = Colors.LightBlue;
		Label4.TextColor = Colors.Black;
	}

	private void Editor_Unfocused(object sender, FocusEventArgs e)
	{
		Label5.BackgroundColor = Colors.LightBlue;
		Label5.TextColor = Colors.Black;
	}
}
