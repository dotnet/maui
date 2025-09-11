using Microsoft.Maui.ManualTests.Categories;

namespace Microsoft.Maui.ManualTests.Tests.TitleBar;

[Test(
	id: "J4",
	title: "Check Window Title when reverting from TitleBar to Default State",
	category: Category.TitleBar)]
public partial class J4 : ContentPage
{
	public J4()
	{
		InitializeComponent();
	}

#if NET9_0_OR_GREATER
	Microsoft.Maui.Controls.TitleBar TitleBar = new Microsoft.Maui.Controls.TitleBar()
	{
		Title = "[TitleBar.TitleBar]",
		Subtitle = "[Title.Subtitle]",
		BackgroundColor = Color.FromRgb(255, 255, 0)
	};
	protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
	{
		base.OnNavigatedFrom(args);

		var window = Application.Current.Windows.FirstOrDefault();

		if (window != null)
		{
			window.Title = "";
			window.TitleBar = null;
		}

	}
	private void SetWindowTitle(object sender, EventArgs e)
	{
		if (this.Window != null)
		{
			this.Window.TitleBar = null;
			this.Window.Title = "Title";
		}
	}

	private void SetWindowTitleBar(object sender, EventArgs e)
	{
		if (this.Window != null)
		{
			this.Window.TitleBar = TitleBar;
		}
	}
#endif
}