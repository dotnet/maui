using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 26066, "CollectionViewHandler2 RelativeSource binding to AncestorType not working", PlatformAffected.Android | PlatformAffected.iOS)]
	public partial class Issue26066 : ContentPage
	{
		public Issue26066()
		{
			InitializeComponent();
		}
	}

	class Issue26066ViewModel
	{
		public Issue26066ViewModel()
		{
			LoadCommand.Execute(null);
			LoadCommandCV2.Execute(null);
		}

		public ObservableCollection<Issue22035Model> Images { get; set; } = new();
		public ObservableCollection<Issue22035Model> Images2 { get; set; } = new();

		public ICommand ShowDialogCommand => new Command(async () => await Application.Current.MainPage.DisplayAlert("New Dialog", "Hello from Espinho", "OK"));

		public ICommand LoadCommand => new Command(() => LoadItems(Images, "CV1"));

		public ICommand LoadCommandCV2 => new Command(() => LoadItems(Images2, "CV2"));

		static void LoadItems(ObservableCollection<Issue22035Model> items, string text)
		{
			items.Clear();
			for (int i = 0; i < 3; i++)
			{
				items.Add(new Issue22035Model { Text = $"{text} - Item {i}", ImagePath = i % 2 == 0 ? "photo21314.jpg" : "oasis.jpg" });
			}
		}
	}
}