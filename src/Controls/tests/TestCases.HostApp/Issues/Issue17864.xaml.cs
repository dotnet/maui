using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 17864, "[Windows] CollectionView throws NRE when value of IsGrouped property is changed to false",
		PlatformAffected.UWP)]
	public partial class Issue17864 : TestContentPage
	{
		readonly Issue17864_ItemListViewModel _ItemListViewModel;
		protected override void Init() { }

		public Issue17864()
		{
			InitializeComponent();
			_ItemListViewModel = new Issue17864_ItemListViewModel();
			BindingContext = _ItemListViewModel;
		}

		private void Button_Clicked(object sender, EventArgs e)
		{
			_ItemListViewModel.IsGrouped = !_ItemListViewModel.IsGrouped;
		}
	}

	internal class Issue17864_GroupViewModel : ObservableCollection<Issue17864_ItemViewModel>, Issue17864_IItemViewModel
	{

		public Issue17864_GroupViewModel(int groupIndex)
		{
			ItemText = $"Group #{groupIndex}";
		}

		public string ItemText { get; }
	}

	public interface Issue17864_IItemViewModel
	{
		string ItemText { get; }
	}

	internal class Issue17864_ItemListViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		ObservableCollection<Issue17864_IItemViewModel> items = new();

		private bool isGrouped = true;


		public Issue17864_ItemListViewModel()
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
				items.Add(new Issue17864_ItemViewModel(itemIndex++));
				items.Add(new Issue17864_ItemViewModel(itemIndex++));
				items.Add(new Issue17864_ItemViewModel(itemIndex++));
			}
		}

		private Issue17864_IItemViewModel GetGroup(int groupIndex, int itemCount)
		{
			var group = new Issue17864_GroupViewModel(groupIndex);
			for (int i = 0; i < itemCount; i++)
			{
				group.Add(new Issue17864_ItemViewModel(i));
			}
			return group;
		}

		public ObservableCollection<Issue17864_IItemViewModel> Items => items;
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

	internal class Issue17864_ItemViewModel : Issue17864_IItemViewModel
	{

		public Issue17864_ItemViewModel(int itemIndex)
		{
			ItemText = $"Item #{itemIndex}";
		}

		public string ItemText { get; }

	}
}