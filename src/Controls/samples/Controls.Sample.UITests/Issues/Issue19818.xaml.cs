using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 19818, "[MAUI] Stuck when entering/exiting G6 page", PlatformAffected.Android)]
	public partial class Issue19818 : ContentPage
	{
		public Issue19818()
		{
			InitializeComponent();

			BindingContext = new Issue19818ViewModel();
		}
	}

	public class Issue19818Model
	{
		public string Name { get; set; }
	}

	public class Issue19818Group : List<Issue19818Model>
	{
		static int counter = 0;

		public static Issue19818Group CreateIssue19818Group()
		{
			counter++;

			return new()
			{
				new Issue19818Model { Name = $"{counter}"},
				new Issue19818Model { Name = $"{counter}"},
				new Issue19818Model { Name = $"{counter}"},
				new Issue19818Model { Name = $"{counter}"},
				new Issue19818Model { Name = $"{counter}"},
				new Issue19818Model { Name = $"{counter}"},
				new Issue19818Model { Name = $"{counter}"},
				new Issue19818Model { Name = $"{counter}"},
				new Issue19818Model { Name = $"{counter}"},
				new Issue19818Model { Name = $"{counter}"},
				new Issue19818Model { Name = $"{counter}"},
				new Issue19818Model { Name = $"{counter}"},
			};
		}
	}

	public class Issue19818ViewModel : BindableObject
	{
		ObservableCollection<Issue19818Group> _groups;

		public ObservableCollection<Issue19818Group> Groups
		{
			get
			{
				return _groups;
			}
			set
			{
				if (_groups != value)
				{
					_groups = value;
					OnPropertyChanged(nameof(Groups));
				}
			}
		}

		public ICommand ThresholdReachedCommand { get; }

		public Issue19818ViewModel()
		{
			_groups = new ObservableCollection<Issue19818Group>()
			{
				Issue19818Group.CreateIssue19818Group(),
				Issue19818Group.CreateIssue19818Group(),
				Issue19818Group.CreateIssue19818Group()
			};

			ThresholdReachedCommand = new Command(ThresholdReachedOperation);
		}

		void ThresholdReachedOperation(object obj)
		{
			Groups.Add(Issue19818Group.CreateIssue19818Group());
		}
	}
}