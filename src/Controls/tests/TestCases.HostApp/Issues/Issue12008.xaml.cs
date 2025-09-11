using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Maui.Controls.Sample.Issues
{
    [Issue(IssueTracker.Github, 12008, "CollectionView Drag and Drop Reordering Can't Drop in Empty Group", PlatformAffected.Android)]
    public partial class Issue12008 : ContentPage
    {
        public Issue12008()
        {
            InitializeComponent();
        }


        private void OnCreateEmptyGroupClicked(object sender, EventArgs e)
        {
            if (this.BindingContext is Issue12008ViewModel viewModel)
            {
                viewModel.CreateEmptyGroup();
                statusLabel.Text = "Status: Empty group created";
            }
        }
    }

    public class Issue12008ViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Issue12008Group> _groupedItems;

        public ObservableCollection<Issue12008Group> GroupedItems
        {
            get => _groupedItems;
            set
            {
                _groupedItems = value;
                OnPropertyChanged();
            }
        }

        public Issue12008ViewModel()
        {
            InitializeData();
        }

        private void InitializeData()
        {
            GroupedItems = new ObservableCollection<Issue12008Group>
            {
                new Issue12008Group("Group A", new List<Issue12008Item>
                {
                    new Issue12008Item("Item A1"),
                    new Issue12008Item("Item A2"),
                    new Issue12008Item("Item A3")
                }),
                new Issue12008Group("Group B", new List<Issue12008Item>
                {
                    new Issue12008Item("Item B1"),
                    new Issue12008Item("Item B2")
                }),
                new Issue12008Group("Group C", new List<Issue12008Item>
                {
                    new Issue12008Item("Item C1")
                })
            };
        }

        public void CreateEmptyGroup()
        {
            var emptyGroup = new Issue12008Group("Empty Group", new List<Issue12008Item>());
            GroupedItems.Add(emptyGroup);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Issue12008Group : ObservableCollection<Issue12008Item>
    {
        public string GroupName { get; }

        public Issue12008Group(string groupName, IEnumerable<Issue12008Item> items) : base(items)
        {
            GroupName = groupName;
        }
    }

    public class Issue12008Item
    {
        public string Name { get; }

        public Issue12008Item(string name)
        {
            Name = name;
        }
    }
}
