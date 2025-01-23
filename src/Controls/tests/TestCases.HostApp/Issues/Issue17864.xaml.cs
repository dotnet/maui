using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 17864, "[Windows] CollectionView throws NRE when value of IsGrouped property is changed to false",
		PlatformAffected.UWP)]
	public partial class Issue17864 : TestContentPage
	{
		readonly ItemListViewModel _ItemListViewModel;
		protected override void Init() { }

		public Issue17864()
		{
			InitializeComponent();
			_ItemListViewModel = new ItemListViewModel();
			BindingContext = _ItemListViewModel;
		}

		private void Button_Clicked(object sender, EventArgs e)
		{
			_ItemListViewModel.IsGrouped = !_ItemListViewModel.IsGrouped;
		}
	}

	internal class GroupViewModel : ObservableCollection<ItemViewModel>, IItemViewModel
	{

		public GroupViewModel(int groupIndex)
		{
			ItemText = $"Group #{groupIndex}";
		}

		public string ItemText { get; }
	}
	
	public interface IItemViewModel
	{
		string ItemText { get; }
	}

	internal class ItemListViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		ObservableCollection<IItemViewModel> items = new();

		private bool isGrouped = true;


		public ItemListViewModel()
		{
			Populate();
		}

		private void Populate()
		{			
			items.Clear();
			if (isGrouped)
			{
				int groupIndex = 0;
				items.Add(GetGroup(groupIndex++, 2));
				items.Add(GetGroup(groupIndex++, 1));
				items.Add(GetGroup(groupIndex++, 3));
				items.Add(GetGroup(groupIndex++, 2));
			}
			else
			{
				int itemIndex = 0;
				items.Add(new ItemViewModel(itemIndex++));
				items.Add(new ItemViewModel(itemIndex++));
				items.Add(new ItemViewModel(itemIndex++));
			}
		}

		private IItemViewModel GetGroup(int groupIndex, int itemCount)
		{
			var group = new GroupViewModel(groupIndex);
			for (int i = 0; i < itemCount; i++)
			{
				group.Add(new ItemViewModel(i));
			}
			return group;
		}

		public ObservableCollection<IItemViewModel> Items => items;
		public bool IsGrouped
		{
			get => isGrouped;
			set
			{
				isGrouped = value;
				Populate();
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsGrouped)));
			}
		}

	}

	internal class ItemViewModel : IItemViewModel
	{

		public ItemViewModel(int itemIndex)
		{
			ItemText = $"Item #{itemIndex}";
		}

		public string ItemText { get; }

	}
}