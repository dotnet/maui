using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 25634, "Picker ItemDisplayBinding doesn't support MVVM properly", PlatformAffected.All)]
	public partial class Issue25634 : ContentPage
	{
		public ObservableCollection<Issue25634_Model> People { get; private set; } = new()
		{
			new Issue25634_Model { FirstName = "John", LastName = "Doe" },
			new Issue25634_Model { FirstName = "Jane", LastName = "Smith" },
			new Issue25634_Model { FirstName = "Sam", LastName = "Johnson" }
		};

		public Issue25634()
		{
			InitializeComponent();
			BindingContext = this;
			picker.SelectedItem = People[0];
		}

		void Button_Clicked(object sender, EventArgs e)
		{
			if (People.Count > 0)
			{
				People[0].LastName = "Alice";
			}
		}
	}
	
	public class Issue25634_Model : INotifyPropertyChanged
	{
		private string firstName = "FirstName";
		private string lastName = "LastName";

		public string FirstName
		{
			get => firstName;
			set
			{
				if (firstName != value)
				{
					firstName = value;
					OnPropertyChanged();
				}
			}
		}

		public string LastName
		{
			get => lastName;
			set
			{
				if (lastName != value)
				{
					lastName = value;
					OnPropertyChanged();
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged([CallerMemberName] string name = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}


    
    