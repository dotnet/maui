using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 25889, "CollectionView RemainingItemsThresholdReachedCommand not Firing", PlatformAffected.iOS)]
	public partial class Issue25889 : ContentPage
	{
		public Issue25889()
		{
			InitializeComponent();
			label.SetBinding(Label.TextProperty, "LabelText");
		}

	}

	public class _25889MainViewModel : BindableObject
	{
		public ObservableCollection<_25889GroupedActivity> ActivityGroups { get; set; }

		public ICommand GetDataCommand { get; }

		private string _labelText;

		public string LabelText
		{
			get => _labelText;
			set
			{
				if (_labelText != value)
				{
					_labelText = value;
					OnPropertyChanged();
				}
			}
		}

		public _25889MainViewModel()
		{

			var activityGroups = new ObservableCollection<_25889GroupedActivity>();

			var group1Items = new List<_25889ActivityItem>();
			for (int i = 1; i <= 10; i++)
			{
				group1Items.Add(new _25889ActivityItem { Name = $"Activity {i}" });
			}
			activityGroups.Add(new _25889GroupedActivity("Group 1", group1Items));

			var group2Items = new List<_25889ActivityItem>();
			for (int i = 11; i <= 25; i++)
			{
				group2Items.Add(new _25889ActivityItem { Name = $"Activity {i}" });
			}
			activityGroups.Add(new _25889GroupedActivity("Group 2", group2Items));

			ActivityGroups = activityGroups;
			GetDataCommand = new Command(RemainingItemsThresholdReachedCommandFired);

			LabelText = "Not fired";
		}

		private void RemainingItemsThresholdReachedCommandFired()
		{
			LabelText = "Command Fired!";
		}
	}

	public class _25889GroupedActivity : List<_25889ActivityItem>
	{
		public string Key { get; }

		public _25889GroupedActivity(string key, List<_25889ActivityItem> items) : base(items)
		{
			Key = key;
		}
	}

	public class _25889ActivityItem
	{
		public string Name { get; set; }
	}
}