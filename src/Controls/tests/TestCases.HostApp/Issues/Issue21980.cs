using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, "21980", "IndicatorView with DataTemplate (custom image) does not render correctly when ItemsSource change.", PlatformAffected.Android | PlatformAffected.iOS)]
	public partial class Issue21980 : ContentPage
	{
		public Issue21980()
		{
			InitializeComponent(); 
			BindingContext = new Issue21980ViewModel();
		}
	}

	public class Issue21980ViewModel : INotifyPropertyChanged
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

		public Command ChangeSourceCommand { get; set; }

		public Issue21980ViewModel()
		{
			Images = _imageSource1;
			ChangeSourceCommand = new Command(() => Images = _imageSource2);
		}

		public event PropertyChangedEventHandler PropertyChanged;
		
		void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}