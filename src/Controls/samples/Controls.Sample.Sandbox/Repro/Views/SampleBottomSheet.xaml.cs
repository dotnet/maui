using The49.Maui.BottomSheet;

namespace AllTheLists.Views;

public partial class SampleBottomSheet : BottomSheet
{
	public SampleBottomSheet()
	{
		InitializeComponent();
	}

	async void CloseButton_Clicked(object sender, EventArgs e)
	{
		await this.DismissAsync();
	}
}