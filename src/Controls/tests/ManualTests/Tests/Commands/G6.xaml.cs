using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.ManualTests.Categories;

namespace Microsoft.Maui.ManualTests.Tests.Commands;

[Test(id: "G6", title: "RemainingItemsThresholdReachedCommand is not called for IsGrouped CollectionViews.", category: Category.Commands)]
public partial class G6 : ContentPage
{
	public G6()
	{
		InitializeComponent();

		BindingContext = new MainPageViewModel();
	}
}

public class Person
{
	public string Name { get; set; }
}

public class StudyGroup : List<Person>
{
	private static int counter = 0;

	public static StudyGroup CreateStudyGroup()
	{
		counter++;
		return new()
		{
			new Person{Name = $"{counter}"},
			new Person{Name = $"{counter}"},
			new Person{Name = $"{counter}"},
			new Person{Name = $"{counter}"},
			new Person{Name = $"{counter}"},
			new Person{Name = $"{counter}"},
			new Person{Name = $"{counter}"},
			new Person{Name = $"{counter}"},
			new Person{Name = $"{counter}"},
			new Person{Name = $"{counter}"},
			new Person{Name = $"{counter}"},
			new Person{Name = $"{counter}"},
		};
	}
}

public class MainPageViewModel : BindableObject
{
	private ObservableCollection<StudyGroup> studyGroups;

	public ObservableCollection<StudyGroup> StudyGroups
	{
		get
		{
			return studyGroups;
		}
		set
		{
			if (studyGroups != value)
			{
				studyGroups = value;
				OnPropertyChanged(nameof(StudyGroups));
			}
		}
	}

	public ICommand ThresholdReachedCommand { get; }

	public MainPageViewModel()
	{
		studyGroups = new ObservableCollection<StudyGroup>()
		{
			StudyGroup.CreateStudyGroup(),
			StudyGroup.CreateStudyGroup(),
			StudyGroup.CreateStudyGroup()
		};

		ThresholdReachedCommand = new Command(ThresholdReachedOperation);
	}

	private void ThresholdReachedOperation(object obj)
	{
		StudyGroups.Add(StudyGroup.CreateStudyGroup());
	}
}
