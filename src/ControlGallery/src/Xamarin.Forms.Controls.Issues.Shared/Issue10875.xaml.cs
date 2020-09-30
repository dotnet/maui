using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.SwipeView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 10875, "[Bug] SwipeView.LeftItems in CollectionView stop Scrolling", PlatformAffected.Android)]
	public partial class Issue10875 : TestContentPage
	{
		public Issue10875()
		{
#if APP
			InitializeComponent();
			BindingContext = new Issue10875ViewModel();
#endif
		}

		protected override void Init()
		{

		}

		void OnSwipeItemViewInvoked(object sender, EventArgs e)
		{
			DisplayAlert("SwipeView", "SwipeItemView Invoked", "Ok");
		}

		void OnCollectionViewSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			DisplayAlert("CollectionViewSwipe", "CollectionView SelectionChanged", "Ok");
		}
	}

	[Preserve(AllMembers = true)]
	public class Issue10875Model
	{
		public string Name { get; set; }
		public string Tier { get; set; }
		public string Category { get; set; }
	}

	[Preserve(AllMembers = true)]
	public class Issue10875ViewModel : BindableObject
	{
		ObservableCollection<Issue10875Model> _fighters;

		public Issue10875ViewModel()
		{
			LoadData();
		}

		public ObservableCollection<Issue10875Model> Fighters
		{
			get { return _fighters; }
			set
			{
				_fighters = value;
				OnPropertyChanged();
			}
		}

		void LoadData()
		{
			Fighters = new ObservableCollection<Issue10875Model>
			{
				new Issue10875Model { Tier = "D TIER", Name = "Fighter 1", Category = "sheet" },
				new Issue10875Model { Tier = "C TIER", Name = "Fighter 2", Category = "sheet" },
				new Issue10875Model { Tier = "E TIER", Name = "Fighter 3", Category = "pokeball" },
				new Issue10875Model { Tier = "D TIER", Name = "Fighter 4", Category = "mushroom" },
				new Issue10875Model { Tier = "E TIER", Name = "Fighter 5", Category = "mushroom" },
				new Issue10875Model { Tier = "B TIER", Name = "Fighter 6", Category = "mushroom" },
				new Issue10875Model { Tier = "D TIER", Name = "Fighter 7", Category = "sheet" },
				new Issue10875Model { Tier = "C TIER", Name = "Fighter 8", Category = "sheet" },
				new Issue10875Model { Tier = "E TIER", Name = "Fighter 9", Category = "pokeball" },
				new Issue10875Model { Tier = "D TIER", Name = "Fighter 9", Category = "mushroom" },
				new Issue10875Model { Tier = "E TIER", Name = "Fighter 10", Category = "mushroom" }
			};
		}
	}
}