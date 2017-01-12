using System;

using Xamarin.Forms.CustomAttributes;
using System.Collections.Generic;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 2976, "Sample 'WorkingWithListviewNative' throw Exception on Xam.Android project.", PlatformAffected.Android)]
	public class Issue2976 : TestTabbedPage 
	{
		protected override void Init ()
		{

			// built-in Xamarin.Forms controls
			Children.Add (new XamarinFormsPage {Title = "DEMOA", Icon = "bank.png"});

			// custom renderer for the list, using a native built-in cell type
			Children.Add (new NativeListPage {Title = "DEMOB", Icon = "bank.png"});

			// built in Xamarin.Forms list, but with a native cell custom-renderer
			Children.Add (new XamarinFormsNativeCellPage {Title = "DEMOC", Icon = "bank.png"});

			// custom renderer for the list, using a native cell that has been custom-defined in native code
			Children.Add (new NativeListViewPage2 {Title = "DEMOD", Icon = "bank.png"});
		}

#if UITEST
		[Test]
		public void Issue1Test ()
		{
			RunningApp.Screenshot ("I am at Issue 2976");
			RunningApp.Tap (q => q.Marked ("DEMOA"));
			RunningApp.Tap (q => q.Marked ("DEMOB"));
			RunningApp.Tap (q => q.Marked ("DEMOC"));
			RunningApp.Tap (q => q.Marked ("DEMOD"));
		}
#endif
	}

	/// <summary>
	/// This page uses a custom renderer that wraps native list controls:
	///    iOS :           UITableView
	///    Android :       ListView   (do not confuse with Xamarin.Forms ListView)
	///    Windows Phone : ?
	/// 
	/// It uses a built-in row/cell class provided by the native platform
	/// and is therefore faster than building a custom ViewCell in Xamarin.Forms.
	/// </summary>
	[Preserve (AllMembers = true)]
	public class NativeListPage : ContentPage
	{
		public NativeListPage ()
		{
			var tableItems = new List<string> ();
			for (var i = 0; i < 100; i++) {
				tableItems.Add (i + " row ");
			}


			var fasterListView = new NativeListView (); // CUSTOM RENDERER using a native control
			fasterListView.VerticalOptions = LayoutOptions.FillAndExpand; // REQUIRED: To share a scrollable view with other views in a StackLayout, it should have a VerticalOptions of FillAndExpand.
			fasterListView.Items = tableItems;
			fasterListView.ItemSelected += async (sender, e) => {
				await Navigation.PushModalAsync (new DetailPage(e.SelectedItem));
			};

			// The root page of your application
			Content = new StackLayout {
				Padding = Device.RuntimePlatform == Device.iOS ? new Thickness(0, 20, 0, 0) : new Thickness(0),
				Children = {
					new Label {
#pragma warning disable 618
						XAlign = TextAlignment.Center,
#pragma warning restore 618
						Text = Device.RuntimePlatform == Device.iOS ? "Custom renderer UITableView" : Device.RuntimePlatform == Device.Android ? "Custom renderer ListView" : "Custom renderer todo"
					},
					fasterListView
				}
			};
		}
	}

	/// <summary>
	/// Xamarin.Forms representation for a custom-renderer that uses 
	/// the native list control on each platform.
	/// </summary>
	public class NativeListView : View
	{
		public static readonly BindableProperty ItemsProperty = 
			BindableProperty.Create ("Items", typeof(IEnumerable<string>), typeof(NativeListView), new List<string>());

		public IEnumerable<string> Items {
			get { return (IEnumerable<string>)GetValue (ItemsProperty); }
			set { SetValue (ItemsProperty, value); } 
		}

		public event EventHandler<SelectedItemChangedEventArgs> ItemSelected;

		public void NotifyItemSelected (object item) {

			if (ItemSelected != null)
				ItemSelected (this, new SelectedItemChangedEventArgs (item));
		}

		public NativeListView ()
		{
		}
	}

	/// <summary>
	/// This page uses built-in Xamarin.Forms controls to display a fast-scrolling list.
	/// 
	/// It uses the built-in <c>TextCell</c> class which does not require special 'layout'
	/// and is therefore faster than building a custom ViewCell in Xamarin.Forms.
	/// </summary>
	[Preserve (AllMembers = true)]
	public class XamarinFormsPage : ContentPage
	{
		public XamarinFormsPage ()
		{
			var tableItems = new List<string> ();
			for (var i = 0; i < 100; i++) {
				tableItems.Add (i + " row ");
			}

			var listView = new ListView ();
			listView.ItemsSource = tableItems;
			listView.ItemTemplate = new DataTemplate(typeof(TextCell));
			listView.ItemTemplate.SetBinding(TextCell.TextProperty, ".");

			listView.ItemSelected += async (sender, e) => {
				if (e.SelectedItem == null)
					return;
				listView.SelectedItem= null; // deselect row
				await Navigation.PushModalAsync (new DetailPage(e.SelectedItem));
			};

			Content = new StackLayout { 
				Padding = Device.RuntimePlatform == Device.iOS ? new Thickness(5, 20, 5, 0) : new Thickness(5,0),
				Children = {
					new Label {
#pragma warning disable 618
						XAlign = TextAlignment.Center,
#pragma warning restore 618
						Text = "Xamarin.Forms built-in ListView"
					},
					listView
				}
			};
		}
	}

	/// <summary>
	/// This page uses built-in Xamarin.Forms controls to display a fast-scrolling list.
	/// 
	/// It uses the built-in <c>TextCell</c> class which does not require special 'layout'
	/// and is therefore faster than building a custom ViewCell in Xamarin.Forms.
	/// </summary>
	[Preserve (AllMembers = true)]
	public class XamarinFormsNativeCellPage : ContentPage
	{
		public XamarinFormsNativeCellPage ()
		{
			var listView = new ListView ();
			listView.ItemsSource = DataSource.GetList ();
			listView.ItemTemplate = new DataTemplate(typeof(NativeCell));

			listView.ItemTemplate.SetBinding(NativeCell.NameProperty, "Name");
			listView.ItemTemplate.SetBinding(NativeCell.CategoryProperty, "Category");
			listView.ItemTemplate.SetBinding(NativeCell.ImageFilenameProperty, "ImageFilename");

			listView.ItemSelected += async (sender, e) => {
				if (e.SelectedItem == null)
					return;
				listView.SelectedItem= null; // deselect row

				await Navigation.PushModalAsync (new DetailPage(e.SelectedItem));
			};

			Content = new StackLayout { 
				Padding = Device.RuntimePlatform == Device.iOS ? new Thickness(0, 20, 0, 0) : new Thickness(0),
				Children = {
					new Label {
#pragma warning disable 618
						XAlign = TextAlignment.Center,
#pragma warning restore 618
						Text = "Xamarin.Forms native Cell" 
					},
					listView
				}
			};
		}
	}

	[Preserve (AllMembers = true)]
	public class NativeCell : ViewCell
	{
		public NativeCell ()
		{
			//View = new ContentView ();
		}

		public static readonly BindableProperty NameProperty = 
			BindableProperty.Create ("Name", typeof(string), typeof(NativeCell), "");
		public string Name {
			get { return (string)GetValue (NameProperty); }
			set { SetValue (NameProperty, value); }
		}


		public static readonly BindableProperty CategoryProperty = 
			BindableProperty.Create ("Category", typeof(string), typeof(NativeCell), "");
		public string Category {
			get { return (string)GetValue (CategoryProperty); }
			set { SetValue (CategoryProperty, value); }
		}


		public static readonly BindableProperty ImageFilenameProperty = 
			BindableProperty.Create ("ImageFilename", typeof(string), typeof(NativeCell), "");
		public string ImageFilename {
			get { return (string)GetValue (ImageFilenameProperty); }
			set { SetValue (ImageFilenameProperty, value); }
		}

	}

	public class DetailPage : ContentPage
	{
		public DetailPage (object detail)
		{
			var l = new Label { Text = "Xamarin.Forms Detail Page" }; 

			var t = new Label ();

			if (detail is string) {
				t.Text = (string)detail;
			} else if (detail is DataSource) {
				t.Text = ((DataSource)detail).Name;
			}

			var b = new Button { Text = "Dismiss" };
			b.Clicked += (sender, e) => Navigation.PopModalAsync();

			Content = new StackLayout { 
				Padding = Device.RuntimePlatform == Device.iOS ? new Thickness(0, 20, 0, 0) : new Thickness(0),
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions =  LayoutOptions.Center,
				Children = {
					l,
					t,
					b
				}
			};
		}
	}

	/// <summary>
	/// This page uses a custom renderer that wraps native list controls:
	///    iOS :           UITableView
	///    Android :       ListView   (do not confuse with Xamarin.Forms ListView)
	///    Windows Phone : ?
	/// 
	/// It uses a CUSTOM row/cell class that is defined natively which 
	/// is still faster than a Xamarin.Forms-defined ViewCell subclass.
	/// </summary>
	[Preserve (AllMembers = true)]
	public class NativeListViewPage2 : ContentPage
	{
		public NativeListViewPage2 ()
		{
			var nativeListView2 = new NativeListView2 (); // CUSTOM RENDERER using a native control

			nativeListView2.VerticalOptions = LayoutOptions.FillAndExpand; // REQUIRED: To share a scrollable view with other views in a StackLayout, it should have a VerticalOptions of FillAndExpand.

			nativeListView2.Items = DataSource.GetList ();

			nativeListView2.ItemSelected += async (sender, e) => {
				//await Navigation.PushModalAsync (new DetailPage(e.SelectedItem));
				await DisplayAlert ("clicked", "one of the rows was clicked", "ok");
			};

			// The root page of your application
			Content = new StackLayout {
				Padding = Device.RuntimePlatform == Device.iOS ? new Thickness(0, 20, 0, 0) : new Thickness(0),
				Children = {
					new Label {
#pragma warning disable 618
						XAlign = TextAlignment.Center,
#pragma warning restore 618
						Text = Device.RuntimePlatform == Device.iOS ? "Custom UITableView+UICell" : Device.RuntimePlatform == Device.Android ? "Custom ListView+Cell" : "Custom renderer todo"
					},
					nativeListView2
				}
			};
		}
	}

	/// <summary>
	/// Xamarin.Forms representation for a custom-renderer that uses 
	/// the native list control on each platform.
	/// </summary>
	public class NativeListView2 : View
	{
		public static readonly BindableProperty ItemsProperty = 
			BindableProperty.Create ("Items", typeof(IEnumerable<DataSource>), typeof(NativeListView2), new List<DataSource>());

		public IEnumerable<DataSource> Items {
			get { return (IEnumerable<DataSource>)GetValue (ItemsProperty); }
			set { SetValue (ItemsProperty, value); } 
		}

		public event EventHandler<SelectedItemChangedEventArgs> ItemSelected;

		public void NotifyItemSelected (object item) {

			if (ItemSelected != null)
				ItemSelected (this, new SelectedItemChangedEventArgs (item));
		}

		public NativeListView2 ()
		{
		}
	}

	[Preserve (AllMembers = true)]
	public class DataSource
	{
		public string Name { get; set; }
		public string Category { get; set; }
		public string ImageFilename { get; set; }

		public DataSource ()
		{
		}

		public DataSource (string name, string category, string imageFilename)
		{
			Name = name;
			Category = category;
			ImageFilename = imageFilename;
		}

		public static List<DataSource> GetList (){
			var l = new List<DataSource> ();


			l.Add (new DataSource ("Asparagus","Vegetables","Vegetables"));
			l.Add (new DataSource ("Avocados","Vegetables","Vegetables"));
			l.Add (new DataSource ("Beetroots","Vegetables","Vegetables"));
			l.Add (new DataSource ("Capsicum","Vegetables","Vegetables"));
			l.Add (new DataSource ("Broccoli","Vegetables","Vegetables"));
			l.Add (new DataSource ("Brussel sprouts","Vegetables","Vegetables"));
			l.Add (new DataSource ("Cabbage","Vegetables","Vegetables"));
			l.Add (new DataSource ("Carrots","Vegetables","Vegetables"));
			l.Add (new DataSource ("Cauliflower","Vegetables","Vegetables"));
			l.Add (new DataSource ("Celery","Vegetables","Vegetables"));
			l.Add (new DataSource ("Corn","Vegetables","Vegetables"));
			l.Add (new DataSource ("Cucumbers","Vegetables","Vegetables"));
			l.Add (new DataSource ("Eggplant","Vegetables","Vegetables"));
			l.Add (new DataSource ("Fennel","Vegetables","Vegetables"));
			l.Add (new DataSource ("Garlic","Vegetables","Vegetables"));
			l.Add (new DataSource ("Beans","Vegetables","Vegetables"));
			l.Add (new DataSource ("Peas","Vegetables","Vegetables"));
			l.Add (new DataSource ("Kale","Vegetables","Vegetables"));
			l.Add (new DataSource ("Leeks","Vegetables","Vegetables"));
			l.Add (new DataSource ("Mushrooms","Vegetables","Vegetables"));
			l.Add (new DataSource ("Olives","Vegetables","Vegetables"));
			l.Add (new DataSource ("Onions","Vegetables","Vegetables"));
			l.Add (new DataSource ("Potatoes","Vegetables","Vegetables"));
			l.Add (new DataSource ("Lettuce","Vegetables","Vegetables"));
			l.Add (new DataSource ("Spinach","Vegetables","Vegetables"));
			l.Add (new DataSource ("Squash","Vegetables","Vegetables"));
			l.Add (new DataSource ("Sweet potatoes","Vegetables","Vegetables"));
			l.Add (new DataSource ("Tomatoes","Vegetables","Vegetables"));
			l.Add (new DataSource ("Turnips","Vegetables","Vegetables"));
			l.Add (new DataSource ("Apples","Fruits","Fruits"));
			l.Add (new DataSource ("Apricots","Fruits","Fruits"));
			l.Add (new DataSource ("Bananas","Fruits","Fruits"));
			l.Add (new DataSource ("Blueberries","Fruits","Fruits"));
			l.Add (new DataSource ("Rockmelon","Fruits","Fruits"));
			l.Add (new DataSource ("Figs","Fruits","Fruits"));
			l.Add (new DataSource ("Grapefruit","Fruits","Fruits"));
			l.Add (new DataSource ("Grapes","Fruits","Fruits"));
			l.Add (new DataSource ("Honeydew Melon","Fruits","Fruits"));
			l.Add (new DataSource ("Kiwifruit","Fruits","Fruits"));
			l.Add (new DataSource ("Lemons","Fruits","Fruits"));
			l.Add (new DataSource ("Oranges","Fruits","Fruits"));
			l.Add (new DataSource ("Pears","Fruits","Fruits"));
			l.Add (new DataSource ("Pineapple","Fruits","Fruits"));
			l.Add (new DataSource ("Plums","Fruits","Fruits"));
			l.Add (new DataSource ("Raspberries","Fruits","Fruits"));
			l.Add (new DataSource ("Strawberries","Fruits","Fruits"));
			l.Add (new DataSource ("Watermelon","Fruits","Fruits"));
			l.Add (new DataSource ("Balmain Bugs","Seafood",""));
			l.Add (new DataSource ("Calamari","Seafood",""));
			l.Add (new DataSource ("Cod","Seafood",""));
			l.Add (new DataSource ("Prawns","Seafood",""));
			l.Add (new DataSource ("Lobster","Seafood",""));
			l.Add (new DataSource ("Salmon","Seafood",""));
			l.Add (new DataSource ("Scallops","Seafood",""));
			l.Add (new DataSource ("Shrimp","Seafood",""));
			l.Add (new DataSource ("Tuna","Seafood",""));
			l.Add (new DataSource ("Almonds","Nuts",""));
			l.Add (new DataSource ("Cashews","Nuts",""));
			l.Add (new DataSource ("Peanuts","Nuts",""));
			l.Add (new DataSource ("Walnuts","Nuts",""));
			l.Add (new DataSource ("Black beans","Beans & Legumes","Legumes"));
			l.Add (new DataSource ("Dried peas","Beans & Legumes","Legumes"));
			l.Add (new DataSource ("Kidney beans","Beans & Legumes","Legumes"));
			l.Add (new DataSource ("Lentils","Beans & Legumes","Legumes"));
			l.Add (new DataSource ("Lima beans","Beans & Legumes","Legumes"));
			l.Add (new DataSource ("Miso","Beans & Legumes","Legumes"));
			l.Add (new DataSource ("Soybeans","Beans & Legumes","Legumes"));
			l.Add (new DataSource ("Beef","Meat",""));
			l.Add (new DataSource ("Buffalo","Meat",""));
			l.Add (new DataSource ("Chicken","Meat",""));
			l.Add (new DataSource ("Lamb","Meat",""));
			l.Add (new DataSource ("Cheese","Dairy",""));
			l.Add (new DataSource ("Milk","Dairy",""));
			l.Add (new DataSource ("Eggs","Dairy",""));
			l.Add (new DataSource ("Basil","Herbs & Spices","FlowerBuds"));
			l.Add (new DataSource ("Black pepper","Herbs & Spices","FlowerBuds"));
			l.Add (new DataSource ("Chili pepper, dried","Herbs & Spices","FlowerBuds"));
			l.Add (new DataSource ("Cinnamon","Herbs & Spices","FlowerBuds"));
			l.Add (new DataSource ("Cloves","Herbs & Spices","FlowerBuds"));
			l.Add (new DataSource ("Cumin","Herbs & Spices","FlowerBuds"));
			l.Add (new DataSource ("Dill","Herbs & Spices","FlowerBuds"));
			l.Add (new DataSource ("Ginger","Herbs & Spices","FlowerBuds"));
			l.Add (new DataSource ("Mustard","Herbs & Spices","FlowerBuds"));
			l.Add (new DataSource ("Oregano","Herbs & Spices","FlowerBuds"));
			l.Add (new DataSource ("Parsley","Herbs & Spices","FlowerBuds"));
			l.Add (new DataSource ("Peppermint","Herbs & Spices","FlowerBuds"));
			l.Add (new DataSource ("Rosemary","Herbs & Spices","FlowerBuds"));
			l.Add (new DataSource ("Sage","Herbs & Spices","FlowerBuds"));
			l.Add (new DataSource ("Thyme","Herbs & Spices","FlowerBuds"));
			l.Add (new DataSource ("Turmeric","Herbs & Spices","FlowerBuds"));


			return l;
		}
	}
}
