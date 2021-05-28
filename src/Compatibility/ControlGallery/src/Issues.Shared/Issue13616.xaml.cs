using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.CarouselView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 13616,
		"[Bug] After updating XF 5.0.0.1931 getting Java.Lang.IllegalArgumentException: Invalid target position at Java.Interop.JniEnvironment+InstanceMethods.CallVoidMethod",
		PlatformAffected.Android)]
	public partial class Issue13616 : TestContentPage
	{
		public Issue13616()
		{
#if APP
			InitializeComponent();
			BindingContext = new Issue13616ViewModel();
#endif
		}

		protected override void Init()
		{
		}

#if UITEST && __ANDROID__
        [Test]
        public void Issue13616Test()
        {
            RunningApp.WaitForElement("AddItemButtonId");
            RunningApp.Tap("AddItemButtonId");
            RunningApp.WaitForElement("CarouselViewId");
        }
#endif
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