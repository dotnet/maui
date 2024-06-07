using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.None, 9075, "FlexLayout trigger Cycle GUI exception", PlatformAffected.UWP)]
	public partial class Issue9075 : ContentPage
	{
		public Issue9075()
		{
			InitializeComponent();

			BindingContext = new Issue9075ViewModel();
		}
	}

	[Preserve(AllMembers = true)]
	public partial class Issue9075ViewModel : BindableObject
	{
		List<int> _carouselCards = new()
		{
			49,
			12,
			50,
			10,
			25,
			9,
			99,
			77
		};

		ObservableCollection<ObservableCollection<string>> _carouselGrids = new();

		public ObservableCollection<ObservableCollection<string>> CarouselGrids
		{
			get => _carouselGrids;
			set
			{
				_carouselGrids = value;
				OnPropertyChanged();
			}
		}

		public Issue9075ViewModel()
		{
			_InitGrids();
		}

		private void _InitGrids()
		{
			foreach (var card in _carouselCards)
			{
				_carouselGrids.Add(new ObservableCollection<string>(Enumerable.Range(1, card).Select(x => $"Item{x}")));
			}
		}
	}
}