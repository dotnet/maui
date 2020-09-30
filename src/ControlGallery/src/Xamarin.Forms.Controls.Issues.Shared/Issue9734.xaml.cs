using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(UITestCategories.CarouselView)]
#endif
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 9734, " SwipeView in 4.5 works only first way swipped", PlatformAffected.Android)]
	public partial class Issue9734 : ContentPage
	{
		public Issue9734()
		{
#if APP
			InitializeComponent();
			BindingContext = new Issue9734ViewModel();
#endif
		}
	}

	public class Issue9734Model
	{
		public string Title { get; set; }
		public string SubTitle { get; set; }
		public string Relation { get; set; }
	}

	public class Issue9734ViewModel : BindableObject
	{
		List<Issue9734Model> _items;

		public Issue9734ViewModel()
		{
			Items = new List<Issue9734Model>();

			LoadItems();
		}

		public List<Issue9734Model> Items
		{
			get { return _items; }
			set
			{
				_items = value;
				OnPropertyChanged();
			}
		}

		void LoadItems()
		{
			var items = new List<Issue9734Model>
			{
				new Issue9734Model { Title = "Test 1 Shop Inc.", SubTitle = "", Relation = "Contractor" },
				new Issue9734Model { Title = "Test 2 Shop Inc.", SubTitle = "", Relation = "Contractor" },
			};

			Items = items;
		}
	}
}