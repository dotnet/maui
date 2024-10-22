using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 25224, "CollectionView - EmptyView with EmptyViewTemplate for Data template selector page throws an exception", PlatformAffected.iOS)]
	public partial class Issue25224
	{
		public Issue25224()
		{
			InitializeComponent();
			BindingContext = new Issue25224ViewModel();
		}
	}
public class SearchTermDataTemplateSelector : DataTemplateSelector
{
	public DataTemplate DefaultTemplate { get; set; }
	public DataTemplate OtherTemplate { get; set; }

	protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
	{
		string query = (string)item;
		return query.Equals("xamarin", StringComparison.OrdinalIgnoreCase) ? OtherTemplate : DefaultTemplate;
	}
}

public class Issue25224ViewModel : INotifyPropertyChanged
{
	readonly IList<Monkey> source;
	public ObservableCollection<Monkey> Monkeys { get; set; }

	public ICommand FilterCommand => new Command<string>(FilterItems);

	public Issue25224ViewModel()
	{
		source = new List<Monkey>();
		CreateMonkeyCollection();
	}

	void CreateMonkeyCollection()
	{
		source.Add(new Monkey
		{
			Name = "Baboon",
			Location = "Africa & Asia",
			Details = "Baboons are African and Arabian Old World monkeys belonging to the genus Papio, part of the subfamily Cercopithecinae.",
			ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/f/fc/Papio_anubis_%28Serengeti%2C_2009%29.jpg/200px-Papio_anubis_%28Serengeti%2C_2009%29.jpg"
		});
		Monkeys = new ObservableCollection<Monkey>(source);
	}

	void FilterItems(string filter)
	{
		var filteredItems = source.Where(monkey => monkey.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
		foreach (var monkey in source)
		{
			if (!filteredItems.Contains(monkey))
			{
				Monkeys.Remove(monkey);
			}
			else
			{
				if (!Monkeys.Contains(monkey))
				{
					Monkeys.Add(monkey);
				}
			}
		}
	}

	#region INotifyPropertyChanged
	public event PropertyChangedEventHandler PropertyChanged;

	void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
	#endregion
}
