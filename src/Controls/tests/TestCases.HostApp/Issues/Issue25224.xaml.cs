#nullable enable
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 25224, "CollectionView - EmptyView with EmptyViewTemplate for Data template selector page throws an exception", PlatformAffected.iOS)]
	public partial class Issue25224: ContentPage
	{
		public Issue25224()
		{
			InitializeComponent();
			var emptyViewTemplateSelector = new SearchTermDataTemplateSelector
			{
				DefaultTemplate = (DataTemplate)Resources["AdvancedTemplate"],
				OtherTemplate = (DataTemplate)Resources["BasicTemplate"]
			};
			collectionView.EmptyViewTemplate = emptyViewTemplateSelector;
			BindingContext = new Issue25224ViewModel();
		}

		public class SearchTermDataTemplateSelector : DataTemplateSelector
		{
			public DataTemplate? DefaultTemplate { get; set; }
			public DataTemplate? OtherTemplate { get; set; }

			protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
			{
				string query = (string)item;
				return query.Equals("xamarin", StringComparison.OrdinalIgnoreCase) ? OtherTemplate : DefaultTemplate;
			}
		}

		internal class Issue25224ViewModel : INotifyPropertyChanged
		{
			private readonly IList<Monkey> source;
			public ObservableCollection<Monkey> Monkeys { get; set; }
			public ICommand FilterCommand => new Command<string>(FilterItems);

			public Issue25224ViewModel()
			{
				source = new List<Monkey>();
				Monkeys = new ObservableCollection<Monkey>(source);
				CreateMonkeyCollection();
			}

			private void CreateMonkeyCollection()
			{
				source.Add(new Monkey
				{
					Name = "Baboon",
					Location = "Africa & Asia",
					Details = "Baboons are African and Arabian Old World monkeys belonging to the genus Papio, part of the subfamily Cercopithecinae."
				});
			}

			private void FilterItems(string filter)
			{
				var filteredItems = source
    .Where(monkey => monkey.Name != null && monkey.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
    .ToList();
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
			public event PropertyChangedEventHandler? PropertyChanged;

			protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName!));
			}
			#endregion
		}
	}
}