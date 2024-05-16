using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 22417, "[MAUI] The list does not show newly added recipes", PlatformAffected.UWP)]
	public partial class Issue22417 : ContentPage
	{
		ObservableCollection<Issue22417Model> _items;
		int _index;

		public Issue22417()
		{
			InitializeComponent();

			Items =
			[
				new Issue22417Model("First", "First CarouselView item"),
				new Issue22417Model("Second", "Second CarouselView item"),
				new Issue22417Model("Third", "Third CarouselView item"),
				new Issue22417Model("Fourth", "Fourth CarouselView item"),
				new Issue22417Model("Fifth", "Fifth CarouselView item"),
			];

			_index = Items.Count + 1;

			BindingContext = this;
		}

		public ObservableCollection<Issue22417Model> Items
		{
			get { return _items; }
			set
			{
				_items = value;
				OnPropertyChanged();
			}
		}

		void OnAddButtonClicked(object sender, System.EventArgs e)
		{
			Items.Add(new Issue22417Model($"Item {_index}", $"CarouselView item {_index}"));
			TestCarousel.ScrollTo(_index);
			_index++;
		}

		public class Issue22417Model
		{
			public Issue22417Model(string title, string description)
			{
				Title = title;
				Description = description;
			}

			public string Title { get; set; }
			public string Description { get; set; }
			public Color Color { get; set; }
		}
	}
}