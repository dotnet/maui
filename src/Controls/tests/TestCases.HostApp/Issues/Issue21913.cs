using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 21913, "Grouped CollectionView items leave stale space when MaximumHeightRequest changes", PlatformAffected.iOS | PlatformAffected.Android)]
public class Issue21913 : ContentPage
{
	const double CollapsedSize = 0;
	const double ExpandedSize = 30;

	public ObservableCollection<Issue21913Group> Groups { get; } = new();

	public Issue21913()
	{
		var group1 = new Issue21913Group
		{
			Name = "One",
			AutomationId = "FirstGroupButton"
		};
		group1.Add(new Issue21913Item("A", "FirstGroupItemA", ExpandedSize));
		group1.Add(new Issue21913Item("B", "FirstGroupItemB", ExpandedSize));
		group1.Add(new Issue21913Item("C", "FirstGroupItemC", ExpandedSize));
		Groups.Add(group1);

		var group2 = new Issue21913Group
		{
			Name = "Two",
			AutomationId = "SecondGroupButton"
		};
		group2.Add(new Issue21913Item("D", "SecondGroupItemD", ExpandedSize));
		group2.Add(new Issue21913Item("E", "SecondGroupItemE", ExpandedSize));
		group2.Add(new Issue21913Item("F", "SecondGroupItemF", ExpandedSize));
		Groups.Add(group2);

		var group3 = new Issue21913Group
		{
			Name = "Three",
			AutomationId = "ThirdGroupButton"
		};
		group3.Add(new Issue21913Item("G", "ThirdGroupItemG", ExpandedSize));
		group3.Add(new Issue21913Item("H", "ThirdGroupItemH", ExpandedSize));
		group3.Add(new Issue21913Item("I", "ThirdGroupItemI", ExpandedSize));
		Groups.Add(group3);

		var collectionView = new CollectionView
		{
			AutomationId = "Issue21913CollectionView",
			IsGrouped = true,
			ItemsSource = Groups,
			GroupHeaderTemplate = new DataTemplate(() =>
			{
				var button = new Button();
				button.SetBinding(Button.TextProperty, nameof(Issue21913Group.Name));
				button.SetBinding(Button.AutomationIdProperty, nameof(Issue21913Group.AutomationId));
				button.SetBinding(Button.CommandProperty, nameof(Issue21913Group.Toggle));
				button.SetBinding(Button.CommandParameterProperty, ".");

				return button;
			}),
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label();
				label.SetBinding(Label.TextProperty, nameof(Issue21913Item.Name));
				label.SetBinding(Label.AutomationIdProperty, nameof(Issue21913Item.AutomationId));
				label.SetBinding(VisualElement.MaximumHeightRequestProperty, nameof(Issue21913Item.Size));

				return label;
			})
		};

		Content = new VerticalStackLayout
		{
			Padding = 12,
			Spacing = 8,
			Children =
			{
				new Label
				{
					Text = "Click Group header to toggle"
				},
				collectionView
			}
		};

		BindingContext = this;
	}

	public class Issue21913Group : ObservableCollection<Issue21913Item>
	{
		public string Name { get; set; }

		public string AutomationId { get; set; }

		public ICommand Toggle => new Command(parameter =>
		{
			if (parameter is Issue21913Group group)
			{
				foreach (var item in group)
					item.Size = item.Size == CollapsedSize ? ExpandedSize : CollapsedSize;
			}
		});
	}

	public class Issue21913Item : INotifyPropertyChanged
	{
		double _size;

		public Issue21913Item(string name, string automationId, double size)
		{
			Name = name;
			AutomationId = automationId;
			_size = size;
		}

		public string Name { get; }

		public string AutomationId { get; }

		public double Size
		{
			get => _size;
			set => SetProperty(ref _size, value);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
		{
			if (Equals(storage, value))
				return false;

			storage = value;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			return true;
		}
	}
}
