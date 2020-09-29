using System;
using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

#if UITEST
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
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
	[Issue(IssueTracker.Github, 7943, "[Android] Crashes if EmptyView defined and ItemsSource is changed after ItemTemplate is changed", PlatformAffected.Android)]
	public partial class Issue7943 : TestContentPage
	{
		public Issue7943()
		{
#if APP
			Title = "Issue 7943";
			InitializeComponent();

			collectionView.ItemTemplate = new DataTemplate(() =>
			{
				var grid = new Grid();
				var lbl1 = new Label();
				lbl1.SetBinding(Label.TextProperty, "Name");
				grid.Children.Add(lbl1);
				var lbl2 = new Label();
				lbl2.SetBinding(Label.TextProperty, "Age");
				lbl2.SetValue(Grid.ColumnProperty, 1);
				grid.Children.Add(lbl2);

				return grid;
			});
			collectionView.ItemsSource = new List<Issue7943Model> { new Issue7943Model("John", 41), new Issue7943Model("Jane", 24) };
#endif
		}

		protected override void Init()
		{

		}


#if APP
		void OnChangeTemplate(object sender, EventArgs e)
		{
			var random = new Random();

			collectionView.ItemTemplate = new DataTemplate(() =>
			{
				var grid = new Grid();
				grid.BackgroundColor = Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255));
				var lbl1 = new Label();
				lbl1.SetBinding(Label.TextProperty, "Name");
				grid.Children.Add(lbl1);
				return grid;
			});
		}
		void OnChangeItemsSource(object sender, EventArgs e)
		{
			collectionView.ItemsSource = new List<Issue7943Model> { new Issue7943Model("Paul", 35), new Issue7943Model("Lucy", 57) };
		}

		void OnClearItemsSource(object sender, EventArgs e)
		{
			collectionView.ItemsSource = null;
		}
#endif
	}

	[Preserve(AllMembers = true)]
	class Issue7943Model
	{
		public string Name { get; set; }
		public int Age { get; set; }
		public Issue7943Model(string name, int age)
		{
			Name = name;
			Age = age;
		}
	}
}