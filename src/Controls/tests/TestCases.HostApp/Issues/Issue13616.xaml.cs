using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 13616,
		"[Bug] After updating XF 5.0.0.1931 getting Java.Lang.IllegalArgumentException: Invalid target position at Java.Interop.JniEnvironment+InstanceMethods.CallVoidMethod",
		PlatformAffected.Android)]
	public partial class Issue13616 : TestContentPage
	{
		public Issue13616()
		{
			InitializeComponent();
			BindingContext = new Issue13616ViewModel();
		}

		protected override void Init()
		{
		}
	}

	[Preserve(AllMembers = true)]
	public class Issue13616Model
	{
		public string Name { get; set; }
		public string Desc { get; set; }
		public Color Color { get; set; }
		public double Scale { get; set; }
	}

	[Preserve(AllMembers = true)]
	public class Issue13616ViewModel : BindableObject
	{
		int _i = 4;
		ObservableCollection<Issue13616Model> _myList { get; set; } = new ObservableCollection<Issue13616Model>();

		public Issue13616ViewModel()
		{
			Items = new ObservableCollection<Issue13616Model>
			{
				new Issue13616Model
				{
					Name = "Card 1",
					Desc = "Card Holder Name 1",
					Color = Colors.Yellow
				},
				new Issue13616Model
				{
					Name = "Card 2",
					Desc = "Card Holder Name 2",
					Color = Colors.Orange
				},
				new Issue13616Model
				{
					Name = "Card 3",
					Desc = "Card Holder Name 3",
					Color = Colors.Red
				}
			};
		}

		public ObservableCollection<Issue13616Model> Items
		{
			get
			{
				return _myList;
			}
			set
			{
				_myList = value;
				OnPropertyChanged(nameof(Items));
			}
		}

		public ICommand AddCardCommand { get { return new Command(AddCard); } }

		void AddCard()
		{
			var tempList = Items.ToList();
			tempList.Add(new Issue13616Model
			{
				Name = "Card " + _i,
				Desc = "Card Holder Name " + _i,
				Color = Colors.Blue
			});
			Items = new ObservableCollection<Issue13616Model>(tempList);
			_i++;
		}
	}
}