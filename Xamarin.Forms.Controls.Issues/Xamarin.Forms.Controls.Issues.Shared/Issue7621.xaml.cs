using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

#if UITEST
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
using System.Linq;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.CollectionView)]
#endif
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7621, "[iOS] MeasureFirstItem is broken for CollectionView", PlatformAffected.iOS)]
	public partial class Issue7621 : TestContentPage
	{
		bool isMeasuringAllItems = false;

#if APP
		public Issue7621()
		{
			InitializeComponent();

			BindingContext = new ViewModel7621();
		}
#endif

		protected override void Init()
		{

		}

		void ButtonClicked(object sender, EventArgs e)
		{
			var button = sender as Button;
			var grid = button.Parent.Parent as Grid;
			var collectionView = grid.Children[1] as CollectionView;

			isMeasuringAllItems = !isMeasuringAllItems;

			collectionView.ItemSizingStrategy = isMeasuringAllItems ? ItemSizingStrategy.MeasureAllItems : ItemSizingStrategy.MeasureFirstItem;
			button.Text = isMeasuringAllItems ? "Switch to MeasureFirstItem" : "Switch to MeasureAllItems";
		}
	}

	[Preserve(AllMembers = true)]
	public class ViewModel7621
	{
		public ObservableCollection<Model7621> Items { get; set; }

		public ViewModel7621()
		{
			var collection = new ObservableCollection<Model7621>();
			Color[] _colors =
			{
				Color.Red,
				Color.Blue,
				Color.Green,
				Color.Yellow
			};
			string[] _images =
			{
				"cover1.jpg",
				"oasis.jpg",
				"photo.jpg",
				"Vegetables.jpg"
			};

			for (var i = 0; i < 30; i++)
			{
				collection.Add(new Model7621
				{
					BackgroundColor = _colors[i % 4],
					Source = _images[i % 4]
				});
			}

			Items = collection;
		}
	}

	[Preserve(AllMembers = true)]
	public class Model7621
	{
		public Color BackgroundColor { get; set; }

		public string Source { get; set; }
	}
}