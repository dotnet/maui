using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
#if APP
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 1545, "Binding instances cannot be reused", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.WinPhone)]
	public partial class Issue1545 : ContentPage
	{
		ArtistsViewModel _viewModel;
		public Issue1545()
		{
			InitializeComponent();
			BindingContext = _viewModel = new ArtistsViewModel();
		}

		protected override void OnAppearing()
		{
		  base.OnAppearing();
		  if (_viewModel.IsInitialized)
			return;

		  _viewModel.IsInitialized = true;
		  _viewModel.LoadCommand.Execute(null);
		}
	}

	public class BaseViewModel : INotifyPropertyChanged
	{
		public string Title { get; set; }
		public bool IsInitialized { get; set; }

		bool _isBusy;

		/// <summary>
		/// Gets or sets if VM is busy working
		/// </summary>
		public bool IsBusy
		{
			get { return _isBusy; }
			set { _isBusy = value; OnPropertyChanged("IsBusy"); }
		}

		//INotifyPropertyChanged Implementation
		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged == null)
			return;

			PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	public class ArtistsViewModel : BaseViewModel
	{
		public ArtistsViewModel()
		{
			Title = "Artists";
			Artists = new ObservableCollection<Artist>();
		}

		/// <summary>
		/// gets or sets the feed items
		/// </summary>
		public ObservableCollection<Artist> Artists
		{
			get;
			private set;
		}

		Command _loadCommand;
		/// <summary>
		/// Command to load/refresh artitists
		/// </summary>
		public Command LoadCommand
		{
			get { return _loadCommand ?? (_loadCommand = new Command(async () => await ExecuteLoadCommand())); }
		}

		static readonly Artist[] ArtistsToLoad = new Artist[] {
			new Artist { Name = "Metallica", ListenerCount = "100", PlayCount = "5000" },
			new Artist { Name = "Epica", ListenerCount = "50", PlayCount = "1000" }
		};

		async Task ExecuteLoadCommand()
		{
			if (IsBusy)
				return;

			IsBusy = true;

			Artists.Clear();

			await Task.Delay (3000);

			foreach (Artist a in ArtistsToLoad)
				Artists.Add (a);

			IsBusy = false;
		}
	}

	public class Artist
	{

		public string Name { get; set; }

		public string PlayCount { get; set; }

		public string ListenerCount { get; set; }

		public string Mbid { get; set; }

		public string Url { get; set; }

		public string Streamable { get; set; }
	}
#endif
}
