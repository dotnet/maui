using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, "21980", "[Android] IndicatorView with DataTemplate (custom image) does not render correctly when ItemsSource change.", PlatformAffected.All)]
	public partial class Issue21980 : ContentPage
	{
		private readonly IReadOnlyList<string> _imageSource1 = new List<string>
		{
			"https://xcdn.next.co.uk/Common/Items/Default/Default/ItemImages/AltItemZoom/440022s.jpg",
			"https://xcdn.next.co.uk/Common/Items/Default/Default/ItemImages/AltItemZoom/440022s2.jpg",
			"https://xcdn.next.co.uk/Common/Items/Default/Default/ItemImages/AltItemZoom/440022s3.jpg",
			"https://xcdn.next.co.uk/Common/Items/Default/Default/ItemImages/AltItemZoom/440022s4.jpg",
			"https://xcdn.next.co.uk/Common/Items/Default/Default/ItemImages/AltItemZoom/440022s5.jpg"
		};
		private readonly IReadOnlyList<string> _imageSource2 = new List<string>()
		{
			"https://xcdn.next.co.uk/Common/Items/Default/Default/ItemImages/AltItemZoom/K92259s.jpg",
			"https://xcdn.next.co.uk/common/Items/Default/Default/ItemImages/AltItemZoom/K92259s2.jpg",
			"https://xcdn.next.co.uk/common/Items/Default/Default/ItemImages/AltItemZoom/K92259s3.jpg",
			"https://xcdn.next.co.uk/common/Items/Default/Default/ItemImages/AltItemZoom/K92259s4.jpg",
			"https://xcdn.next.co.uk/common/Items/Default/Default/ItemImages/AltItemZoom/K92259s5.jpg",
			"https://xcdn.next.co.uk/common/Items/Default/Default/ItemImages/AltItemZoom/K92259s6.jpg"
		};

		private IReadOnlyList<string> _images = [];
		public IReadOnlyList<string> Images
		{
			get => _images;
			set
			{
				_images = value;
				OnPropertyChanged();
			}
		}

		public Issue21980()
		{
			InitializeComponent();
			BindingContext = this;
		}

		private void OnChangeSourceClicked(object sender, EventArgs e)
		{
			Images = _imageSource2;
		}
	}
}