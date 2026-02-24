using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 27797, "CollectionView with grouped data crashes on iOS when the groups change", PlatformAffected.iOS)]
public class Issue27797NavigationPage : NavigationPage
{
	public Issue27797NavigationPage() : base(new Issue27797()) { }
}

public partial class Issue27797 : ContentPage
{
	public Issue27797()
	{
		InitializeComponent();
		BindingContext = new MainViewModel();
	}

	private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
	{
		DetailPage detailPage = new();
		var workItemViewModel = (sender as VisualElement).BindingContext;
		detailPage.BindingContext = workItemViewModel;
		Navigation.PushAsync(detailPage);
	}

	public class DetailPage : ContentPage
	{
		public DetailPage()
		{
			Title = "DetailPage";
			BindingContext = new WorkItemViewModel();

			var statusLabel = new Label
			{
				FontAttributes = FontAttributes.Bold,
				FontSize = 20,
				HorizontalOptions = LayoutOptions.Center
			};
			statusLabel.SetBinding(Label.TextProperty, "Status");

			var descriptionLabel = new Label
			{
				FontSize = 16,
				HorizontalOptions = LayoutOptions.Center
			};
			descriptionLabel.SetBinding(Label.TextProperty, "Description");

			var todoButton = new Button
			{
				Text = "TODO",
				AutomationId = "TODO"
			};
			todoButton.SetBinding(Button.CommandProperty, "ChangeStatusCommand");
			todoButton.SetBinding(Button.CommandParameterProperty, new Binding { Source = "TODO" });

			var activeButton = new Button
			{
				Text = "ACTIVE",
				AutomationId = "ACTIVE"
			};
			activeButton.SetBinding(Button.CommandProperty, "ChangeStatusCommand");
			activeButton.SetBinding(Button.CommandParameterProperty, new Binding { Source = "ACTIVE" });

			var doneButton = new Button
			{
				Text = "DONE",
				AutomationId = "DONE",
			};
			doneButton.SetBinding(Button.CommandProperty, "ChangeStatusCommand");
			doneButton.SetBinding(Button.CommandParameterProperty, new Binding { Source = "DONE" });

			var buttonLayout = new HorizontalStackLayout
			{
				HorizontalOptions = LayoutOptions.Center,
				Spacing = 5,
				Children = { todoButton, activeButton, doneButton }
			};

			var mainLayout = new VerticalStackLayout
			{
				Spacing = 10,
				Children = { statusLabel, descriptionLabel, buttonLayout }
			};

			Content = mainLayout;
		}
	}

	public class MainViewModel : BindableObject
	{
		private List<WorkItemViewModel> _workItems = new();

		private ObservableCollection<WorkItemGroupViewModel> _groupedWorkItems = new();
		public ObservableCollection<WorkItemGroupViewModel> GroupedWorkItems
		{
			get => _groupedWorkItems;
			set { _groupedWorkItems = value; OnPropertyChanged(); }
		}

		static readonly string StatusTODO = "TODO";
		static readonly string StatusACTIVE = "ACTIVE";
		static readonly string StatusDONE = "DONE";

		WorkItemGroupViewModel _todoGroup = new(StatusTODO);
		WorkItemGroupViewModel _activeGroup = new(StatusACTIVE);
		WorkItemGroupViewModel _doneGroup = new(StatusDONE);

		public MainViewModel()
		{
			CreateWorkItemData();
			UpdateGroupedWorkItems();
		}

		private void UpdateGroupedWorkItems()
		{
			foreach (var item in _workItems)
			{
				if (item.Status == StatusTODO && !_todoGroup.Contains(item))
				{
					_todoGroup.Add(item);
				}
				else if (item.Status == StatusACTIVE && !_activeGroup.Contains(item))
				{
					// move any existing active item back to TODO
					if (_activeGroup.Any())
					{
						var currentActiveItem = _activeGroup[0];
						_activeGroup.Remove(currentActiveItem);
						currentActiveItem.Status = StatusTODO;
						_todoGroup.Add(currentActiveItem);
					}
					_activeGroup.Add(item);
				}
				else if (item.Status == StatusDONE && !_doneGroup.Contains(item))
				{
					_doneGroup.Add(item);
				}
			}

			if (_todoGroup.Any())
			{
				if (!GroupedWorkItems.Contains(_todoGroup))
					GroupedWorkItems.Add(_todoGroup);
			}
			else if (GroupedWorkItems.Contains(_todoGroup))
			{
				GroupedWorkItems.Remove(_todoGroup);
			}

			if (_activeGroup.Any())
			{
				// ACTIVE always at top
				if (!GroupedWorkItems.Contains(_activeGroup))
					GroupedWorkItems.Insert(0, _activeGroup);
			}
			else if (GroupedWorkItems.Contains(_activeGroup))
			{
				GroupedWorkItems.Remove(_activeGroup);
			}

			if (_doneGroup.Any())
			{
				if (!GroupedWorkItems.Contains(_doneGroup))
					GroupedWorkItems.Add(_doneGroup);
			}
			else if (GroupedWorkItems.Contains(_doneGroup))
			{
				GroupedWorkItems.Remove(_doneGroup);
			}
		}

		private bool _statusUpdatesInProgress = false;
		private void WorkItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (_statusUpdatesInProgress)
				return;

			if (sender is not WorkItemViewModel workItem)
				return;

			if (e.PropertyName == nameof(WorkItemViewModel.Status))
			{
				_statusUpdatesInProgress = true;
				RemoveItemFromOldGroup(workItem);

				UpdateGroupedWorkItems();
				_statusUpdatesInProgress = false;
			}
		}

		private void RemoveItemFromOldGroup(WorkItemViewModel workItem)
		{
			if (_todoGroup.Contains(workItem))
				_todoGroup.Remove(workItem);

			if (_activeGroup.Contains(workItem))
				_activeGroup.Remove(workItem);

			if (_doneGroup.Contains(workItem))
				_doneGroup.Remove(workItem);
		}

		private void CreateWorkItemData()
		{
			var cleanHouse = new WorkItemViewModel
			{
				Description = "CleanHouse",
				Status = StatusDONE
			};
			cleanHouse.PropertyChanged += WorkItem_PropertyChanged;
			_workItems.Add(cleanHouse);

			var doLaundry = new WorkItemViewModel
			{
				Description = "DoLaundry",
				Status = StatusDONE
			};
			doLaundry.PropertyChanged += WorkItem_PropertyChanged;
			_workItems.Add(doLaundry);

			var mowLawn = new WorkItemViewModel
			{
				Description = "MowLawn",
				Status = StatusTODO
			};
			mowLawn.PropertyChanged += WorkItem_PropertyChanged;
			_workItems.Add(mowLawn);

		}
	}

	public class WorkItemGroupViewModel : ObservableCollection<WorkItemViewModel>
	{
		public WorkItemGroupViewModel(string groupDescription)
		{
			GroupDescription = groupDescription;
		}

		public string GroupDescription { get; }
	}

	public class WorkItemViewModel : BindableObject
	{
		private string _description;
		public string Description
		{
			get => _description;
			set { _description = value; OnPropertyChanged(); }
		}

		private string _status;
		public string Status
		{
			get => _status;
			set { _status = value; OnPropertyChanged(); }
		}
		public Command<string> ChangeStatusCommand => new((newStatus) => Status = newStatus);
	}
}