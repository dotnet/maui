using Microsoft.Maui.ManualTests.Categories;

namespace Microsoft.Maui.ManualTests.Tests.TitleBar;

[Test(
	id: "J3",
	title: "Check Window.TitleBar is not reset after changing Window.Page at runtime.",
	category: Category.TitleBar)]
public partial class J3 : ContentPage
{
	public static Page FirstPage;
	public J3()
	{
		InitializeComponent();
		this.ShowFirstPageBtn.IsEnabled = FirstPage != null;
	}

#if NET9_0_OR_GREATER
	Microsoft.Maui.Controls.TitleBar TitleBar = new Microsoft.Maui.Controls.TitleBar()
	{
		Title = "[TitleBar.TitleBar]",
		Subtitle = "[Title.Subtitle]",
		BackgroundColor = Color.FromRgb(255, 255, 0)
	};

	protected override void OnNavigatedTo(NavigatedToEventArgs args)
	{
		if (this.Window is null)
			return;
		FirstPage = Window.Page;

		this.Window.TitleBar = TitleBar;
	}

	protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
	{
		base.OnNavigatedFrom(args);

		FirstPage = null;

		var window = Application.Current.Windows.FirstOrDefault();

		window?.TitleBar = null;
	}
#endif

	private void AssignNewPage(object sender, EventArgs e)
	{
		var page = new J3()
		{
			BackgroundColor = (BackgroundColor == Colors.White) ? Colors.Pink : Colors.White
		};

		this.Window.Page = page;
	}

	private void ShowFirstPage(object sender, EventArgs e)
	{
		this.Window.Page = FirstPage;
	}
}