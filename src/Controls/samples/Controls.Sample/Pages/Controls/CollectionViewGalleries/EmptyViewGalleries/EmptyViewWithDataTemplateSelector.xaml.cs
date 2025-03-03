#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

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

		public class Monkey
		{
			public string? Name { get; set; }
			public string? Location { get; set; }
			public string? Details { get; set; }
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

		internal class EmptyViewWithDataTemplateSelectorViewModel
		{
			public ObservableCollection<Monkey> Monkeys { get; } = new();
			public ICommand FilterCommand => new Command<string>(FilterItems);

			public EmptyViewWithDataTemplateSelectorViewModel()
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