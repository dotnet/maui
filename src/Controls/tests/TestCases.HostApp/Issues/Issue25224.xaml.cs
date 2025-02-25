#nullable enable
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 25224, "CollectionView - EmptyView with EmptyViewTemplate for Data template selector page throws an exception", PlatformAffected.iOS)]
	public partial class Issue25224 : ContentPage
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

		internal class Issue25224ViewModel
		{
			public ObservableCollection<Monkey> Monkeys { get; } = new();
			public ICommand FilterCommand => new Command<string>(FilterItems);

			public Issue25224ViewModel()
			{
				// Directly populate the ObservableCollection
				Monkeys.Add(new Monkey
				{
					Name = "Baboon",
					Location = "Africa & Asia",
					Details = "Baboons are African and Arabian Old World monkeys belonging to the genus Papio, part of the subfamily Cercopithecinae."
				});
			}

			private void FilterItems(string filter)
			{
				var filteredItems = Monkeys.Where(monkey => monkey.Name?.Contains(filter, StringComparison.OrdinalIgnoreCase) ?? false).ToList();
				Monkeys.Clear();
				foreach (var monkey in filteredItems)
				{
					Monkeys.Add(monkey);
				}
			}
		}
	}
}