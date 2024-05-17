using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.CustomAttributes;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Issue(IssueTracker.Github, 13437, "[Bug] Changing ItemsLayout of CollectionView at runtime does not work on UWP",
		PlatformAffected.UWP)]
	public partial class Issue13437 : TestContentPage
	{
		public Issue13437()
		{
#if APP
			InitializeComponent();

			ButtonOne.Text = "Set Vertical List";
			ButtonOne.Command = new Command(() =>
			{
				SetVerticalList();
			});

			ButtonTwo.Text = "Set Horizontal List";
			ButtonTwo.Command = new Command(() =>
			{
				SetHorizontalList();
			});

			ButtonThree.Text = "Set Grid 2 List";
			ButtonThree.Command = new Command(() =>
			{
				SetTwoGrid();
			});

			ButtonFour.Text = "Set Grid 3 List";
			ButtonFour.Command = new Command(() =>
			{
				SetThreeGrid();
			});

			var collection = new ObservableCollection<Issue13437Model>();

			for (int i = 0; i < 42; i++)
			{
				collection.Add(new Issue13437Model { Text = "Label " + i.ToString() });
			}

			Collection.ItemsSource = collection;

			BindingContext = new ViewModel10482();
#endif
		}

		protected override void Init()
		{

		}
#if APP
		void SetVerticalList()
		{
			Collection.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical);
		}

		void SetHorizontalList()
		{
			Collection.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal);
		}

		void SetTwoGrid()
		{
			Collection.ItemsLayout = new GridItemsLayout(ItemsLayoutOrientation.Vertical)
			{
				Span = 2,
				HorizontalItemSpacing = 5,
				VerticalItemSpacing = 5
			};
		}

		void SetThreeGrid()
		{
			Collection.ItemsLayout = new GridItemsLayout(3, ItemsLayoutOrientation.Vertical);
		}
#endif
	}

	public class Issue13437Model
	{
		public string Text { get; set; }
	}
}