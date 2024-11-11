using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Maui.Controls.Sample.Pages.CollectionViewGalleries.EmptyViewGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class EmptyViewWithDataTemplateSelector : ContentPage
	{
		public EmptyViewWithDataTemplateSelector()
		{
			InitializeComponent();
			var emptyViewTemplateSelector = new SearchTermDataTemplateSelector
			{
				DefaultTemplate = (DataTemplate)Resources["AdvancedTemplate"],
				OtherTemplate = (DataTemplate)Resources["BasicTemplate"]
			};
			collectionView.EmptyViewTemplate = emptyViewTemplateSelector;
			BindingContext = new EmptyViewWithDataTemplateSelectorViewModel();
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

		internal class EmptyViewWithDataTemplateSelectorViewModel : INotifyPropertyChanged
		{
			private readonly IList<Monkey> source;
			public ObservableCollection<Monkey> Monkeys { get; set; }
			public ICommand FilterCommand => new Command<string>(FilterItems);

			public EmptyViewWithDataTemplateSelectorViewModel()
			{
				source = new List<Monkey>();
				CreateMonkeyCollection();
			}

			public partial class Monkey
			{
				public string Name { get; set; }

				public string Location { get; set; }

				public string Details { get; set; }

			}

			private void CreateMonkeyCollection()
			{
				source.Add(new Monkey
				{
					Name = "Baboon",
					Location = "Africa & Asia",
					Details = "Baboons are African and Arabian Old World monkeys belonging to the genus Papio, part of the subfamily Cercopithecinae."
				});
				Monkeys = new ObservableCollection<Monkey>(source);
			}

			private void FilterItems(string filter)
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

			protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			}
			#endregion
		}
	}
}