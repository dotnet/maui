using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 19379, "Not able to update CollectionView header", PlatformAffected.iOS)]
	public partial class Issue19379 : ContentPage
	{
		int _initValue;
		IList<string> _itemList;
		Issue19379CustomHeader _customHeader;

		public Issue19379()
		{
			InitializeComponent();
			BindingContext = this;

			PopulateList();
			CustomHeader = new Issue19379CustomHeader
			{
				Title = "This is a CollectionViewHeader"
			};
		}

		void Button_Clicked(object sender, EventArgs e)
		{
			_initValue++;
			CustomHeader = new Issue19379CustomHeader
			{
				Title = $"This is a CollectionViewHeader #{_initValue}"
			};
		}

		void PopulateList()
		{
			ItemList = new List<string>()
			{
				"This is an item",
				"This is an item",
				"This is an item",
				"This is an item",
				"This is an item",
				"This is an item"
			};
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
		}



		public IList<string> ItemList
		{
			get { return _itemList; }
			set
			{
				_itemList = value;
				OnPropertyChanged();
			}
		}

		public Issue19379CustomHeader CustomHeader
		{
			get { return _customHeader; }
			set
			{
				_customHeader = value;
				OnPropertyChanged();
			}
		}
	}

	public class Issue19379CustomHeader
	{
		public string Title { get; set; }
	}
}