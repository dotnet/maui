namespace Xamarin.Forms.Controls
{
	public class ImageCellTablePage : ContentPage
	{

		public ImageCellTablePage ()
		{
			Title = "ImageCell Table Gallery - Legacy";

			if (Device.RuntimePlatform == Device.iOS && Device.Idiom == TargetIdiom.Tablet)
				Padding = new Thickness(0, 0, 0, 60);

			var tableSection = new TableSection ("Section One") {
				new ImageCell { Text = "Text 1", ImageSource = new FileImageSource { File = "crimson.jpg" } },
				new ImageCell { Text = "Text 2", Detail = "Detail 1", ImageSource = new FileImageSource { File = "crimson.jpg" } },
				new ImageCell { Text = "Text 3", ImageSource = new FileImageSource { File = "cover1.jpg" } },
				new ImageCell { Text = "Text 4", Detail = "Detail 2", ImageSource = new FileImageSource { File = "cover1.jpg" } },
				new ImageCell { Text = "Text 5", ImageSource = new FileImageSource { File = "oasis.jpg" } },
				new ImageCell { Text = "Text 6", Detail = "Detail 3", ImageSource = new FileImageSource { File = "oasis.jpg" } },
				new ImageCell { Text = "Text 7", ImageSource = new FileImageSource { File = "crimson.jpg" } },
				new ImageCell { Text = "Text 8", Detail = "Detail 4", ImageSource = new FileImageSource { File = "crimson.jpg" } },
				new ImageCell { Text = "Text 9", ImageSource = new FileImageSource { File = "cover1.jpg" } },
				new ImageCell { Text = "Text 10", Detail = "Detail 5", ImageSource = new FileImageSource { File = "cover1.jpg" } },
				new ImageCell { Text = "Text 11", ImageSource = new FileImageSource { File = "oasis.jpg" } },
				new ImageCell { Text = "Text 12", Detail = "Detail 6", ImageSource = new FileImageSource { File = "oasis.jpg" } },
			};

			ImageCell imageCell = null;
			imageCell = new ImageCell { 
				Text = "not tapped",
				ImageSource = "oasis.jpg",
				Command = new Command(()=>{
					imageCell.Text = "tapped";
					(imageCell.ImageSource as FileImageSource).File = "crimson.jpg";
				})
			};
			var tableSectionTwo = new TableSection ("Section Two") {
				new ImageCell { Text = "Text 13", ImageSource = new FileImageSource { File = "crimson.jpg" } },
				new ImageCell { Text = "Text 14", Detail = "Detail 7", ImageSource = new FileImageSource { File = "crimson.jpg" } },
				new ImageCell { Text = "Text 15", ImageSource = new FileImageSource { File = "cover1.jpg" } },
				new ImageCell { Text = "Text 16", Detail = "Detail 8", ImageSource = new FileImageSource { File = "cover1.jpg" } },
				new ImageCell { Text = "Text 17", ImageSource = new FileImageSource { File = "oasis.jpg" } },
				new ImageCell { Text = "Text 18", Detail = "Detail 9", ImageSource = new FileImageSource { File = "oasis.jpg" } },
				new ImageCell { Text = "Text 19", ImageSource = new FileImageSource { File = "crimson.jpg" } },
				new ImageCell { Text = "Text 20", Detail = "Detail 10", ImageSource = new FileImageSource { File = "crimson.jpg" } },
				new ImageCell { Text = "Text 21", ImageSource = new FileImageSource { File = "cover1.jpg" } },
				new ImageCell { Text = "Text 22", Detail = "Detail 11", ImageSource = new FileImageSource { File = "cover1.jpg" } },
				new ImageCell { Text = "Text 23", ImageSource = new FileImageSource { File = "oasis.jpg" } },
				new ImageCell { Text = "Text 24", Detail = "Detail 12", ImageSource = new FileImageSource { File = "oasis.jpg" } },
				imageCell,
			};

			var root = new TableRoot ("Text Cell table") {
				tableSection,
				tableSectionTwo
			};

			var table = new TableView {
				AutomationId = CellTypeList.CellTestContainerId,
				Root = root,
			};

			Content = table;
		}
	}
}