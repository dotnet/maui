namespace AllTheLists.Views;

public partial class LearningUnitListItem : ContentView
{
	public LearningUnitListItem()
	{
		InitializeComponent();
	}

	private void OnChapters_Clicked(object sender, EventArgs e)
	{
		ChaptersLayout.IsVisible = !ChaptersLayout.IsVisible;
	}
}