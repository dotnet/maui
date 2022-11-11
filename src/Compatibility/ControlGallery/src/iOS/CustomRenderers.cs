using System;
using System.Collections.Generic;
using System.ComponentModel;
using CoreGraphics;
using CoreLocation;
using Foundation;
using MapKit;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Controls.Platform;
using ObjCRuntime;
using UIKit;
using RectangleF = CoreGraphics.CGRect;

[assembly: ExportRenderer(typeof(Bugzilla21177.CollectionView), typeof(Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS.CollectionViewRenderer))]
[assembly: ExportRenderer(typeof(NativeCell), typeof(NativeiOSCellRenderer))]
[assembly: ExportRenderer(typeof(NativeListView2), typeof(NativeiOSListViewRenderer))]
[assembly: ExportRenderer(typeof(NativeListView), typeof(NativeListViewRenderer))]
[assembly: ExportRenderer(typeof(Bugzilla39987.CustomMapView), typeof(CustomIOSMapRenderer))]
[assembly: ExportRenderer(typeof(TabbedPage), typeof(TabbedPageWithCustomBarColorRenderer))]
[assembly: ExportRenderer(typeof(Bugzilla43161.AccessoryViewCell), typeof(AccessoryViewCellRenderer))]
[assembly: ExportRenderer(typeof(Bugzilla36802.AccessoryViewCell), typeof(AccessoryViewCellRenderer))]
[assembly: ExportRenderer(typeof(Bugzilla52700.NoSelectionViewCell), typeof(NoSelectionViewCellRenderer))]
#pragma warning disable CS0612 // Type or member is obsolete
[assembly: ExportRenderer(typeof(Bugzilla31395.CustomContentView), typeof(CustomContentRenderer))]
[assembly: ExportRenderer(typeof(Issue1683.EntryKeyboardFlags), typeof(EntryRendererKeyboardFlags))]
[assembly: ExportRenderer(typeof(Issue1683.EditorKeyboardFlags), typeof(EditorRendererKeyboardFlags))]
#pragma warning restore CS0612 // Type or member is obsolete
[assembly: ExportRenderer(typeof(Issue5830.ExtendedEntryCell), typeof(ExtendedEntryCellRenderer))]
[assembly: ExportRenderer(typeof(Issue13390), typeof(Issue13390Renderer))]
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS
{
	public class Issue13390Renderer : Controls.Handlers.Compatibility.ShellRenderer
	{
		protected override Controls.Platform.Compatibility.IShellFlyoutRenderer CreateFlyoutRenderer()
		{
			return new Controls.Platform.Compatibility.ShellFlyoutRenderer()
			{
				FlyoutTransition = new SlideFlyoutTransition2()
			};
		}

		public class SlideFlyoutTransition2 : Controls.Platform.Compatibility.IShellFlyoutTransition
		{
			public void LayoutViews(CGRect bounds, nfloat openPercent, UIView flyout, UIView shell, FlyoutBehavior behavior)
			{
				flyout.Frame = new CGRect(0, 0, 0, 0);
			}
		}
	}

	public class CustomIOSMapRenderer : Handlers.Compatibility.ViewRenderer<Bugzilla39987.CustomMapView, MKMapView>
	{
		private MKMapView _mapView;

		protected override void OnElementChanged(ElementChangedEventArgs<Bugzilla39987.CustomMapView> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					_mapView = new MKMapView(UIScreen.MainScreen.Bounds);
					_mapView.MapType = MKMapType.Standard;
					_mapView.RotateEnabled = false;
					SetNativeControl(_mapView);
				}

			}

			CLLocationCoordinate2D coords = new CLLocationCoordinate2D(48.857, 2.351);
			MKCoordinateSpan span = new MKCoordinateSpan(MilesToLatitudeDegrees(20), MilesToLongitudeDegrees(20, coords.Latitude));
			_mapView.Region = new MKCoordinateRegion(coords, span);
		}

		public double MilesToLatitudeDegrees(double miles)
		{
			double earthRadius = 3960.0; // in miles
			double radiansToDegrees = 180.0 / Math.PI;
			return (miles / earthRadius) * radiansToDegrees;
		}

		public double MilesToLongitudeDegrees(double miles, double atLatitude)
		{
			double earthRadius = 3960.0; // in miles
			double degreesToRadians = Math.PI / 180.0;
			double radiansToDegrees = 180.0 / Math.PI;
			// derive the earth's radius at that point in latitude
			double radiusAtLatitude = earthRadius * Math.Cos(atLatitude * degreesToRadians);
			return (miles / radiusAtLatitude) * radiansToDegrees;
		}
	}


	public class NativeiOSCellRenderer : Handlers.Compatibility.ViewCellRenderer
	{
		static NSString s_rid = new NSString("NativeCell");

		public NativeiOSCellRenderer()
		{
		}

		public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
		{
			var x = (NativeCell)item;
			Console.WriteLine(x);

			NativeiOSCell c = reusableCell as NativeiOSCell;

			if (c == null)
			{
				c = new NativeiOSCell(s_rid);
			}

			UIImage i = null;
			if (!string.IsNullOrWhiteSpace(x.ImageFilename))
			{
				i = UIImage.FromFile("Images/" + x.ImageFilename + ".jpg");
			}

			base.WireUpForceUpdateSizeRequested(item, c, tv);

			c.UpdateCell(x.Name, x.Category, i);

			return c;
		}
	}


	/// <summary>
	/// Sample of a custom cell layout, taken from the iOS docs at
	/// http://developer.xamarin.com/guides/ios/user_interface/tables/part_3_-_customizing_a_table's_appearance/
	/// </summary>
	public class NativeiOSCell : UITableViewCell
	{
		UILabel _headingLabel;
		UILabel _subheadingLabel;
		UIImageView _imageView;

		public NativeiOSCell(NSString cellId) : base(UITableViewCellStyle.Default, cellId)
		{
			SelectionStyle = UITableViewCellSelectionStyle.Gray;

			ContentView.BackgroundColor = UIColor.FromRGB(255, 255, 224);

			_imageView = new UIImageView();

			_headingLabel = new UILabel()
			{
				Font = UIFont.FromName("Cochin-BoldItalic", 22f),
				TextColor = UIColor.FromRGB(127, 51, 0),
				BackgroundColor = UIColor.Clear
			};

			_subheadingLabel = new UILabel()
			{
				Font = UIFont.FromName("AmericanTypewriter", 12f),
				TextColor = UIColor.FromRGB(38, 127, 0),
				TextAlignment = UITextAlignment.Center,
				BackgroundColor = UIColor.Clear
			};

			ContentView.Add(_headingLabel);
			ContentView.Add(_subheadingLabel);
			ContentView.Add(_imageView);
		}

		public void UpdateCell(string caption, string subtitle, UIImage image)
		{
			_imageView.Image = image;
			_headingLabel.Text = caption;
			_subheadingLabel.Text = subtitle;
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			_imageView.Frame = new RectangleF(ContentView.Bounds.Width - 63, 5, 33, 33);
			_headingLabel.Frame = new RectangleF(5, 4, ContentView.Bounds.Width - 63, 25);
			_subheadingLabel.Frame = new RectangleF(100, 18, 100, 20);
		}
	}

	/// <summary>
	/// Sample of a custom cell layout, taken from the iOS docs at
	/// http://developer.xamarin.com/guides/ios/user_interface/tables/part_3_-_customizing_a_table's_appearance/
	/// </summary>
	public class NativeiOSListViewCell : UITableViewCell
	{
		UILabel _headingLabel;
		UILabel _subheadingLabel;
		UIImageView _imageView;

		public NativeiOSListViewCell(NSString cellId) : base(UITableViewCellStyle.Default, cellId)
		{
			SelectionStyle = UITableViewCellSelectionStyle.Gray;

			ContentView.BackgroundColor = UIColor.FromRGB(218, 255, 127);

			_imageView = new UIImageView();

			_headingLabel = new UILabel()
			{
				Font = UIFont.FromName("Cochin-BoldItalic", 22f),
				TextColor = UIColor.FromRGB(127, 51, 0),
				BackgroundColor = UIColor.Clear
			};

			_subheadingLabel = new UILabel()
			{
				Font = UIFont.FromName("AmericanTypewriter", 12f),
				TextColor = UIColor.FromRGB(38, 127, 0),
				TextAlignment = UITextAlignment.Center,
				BackgroundColor = UIColor.Clear
			};

			ContentView.Add(_headingLabel);
			ContentView.Add(_subheadingLabel);
			ContentView.Add(_imageView);
		}

		public void UpdateCell(string caption, string subtitle, UIImage image)
		{
			_imageView.Image = image;
			_headingLabel.Text = caption;
			_subheadingLabel.Text = subtitle;
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			_imageView.Frame = new RectangleF(ContentView.Bounds.Width - 63, 5, 33, 33);
			_headingLabel.Frame = new RectangleF(5, 4, ContentView.Bounds.Width - 63, 25);
			_subheadingLabel.Frame = new RectangleF(100, 18, 100, 20);
		}
	}

	public class NativeiOSListViewRenderer : Handlers.Compatibility.ViewRenderer<NativeListView2, UITableView>
	{
		public NativeiOSListViewRenderer()
		{
		}

		protected override void OnElementChanged(ElementChangedEventArgs<NativeListView2> e)
		{
			base.OnElementChanged(e);

			if (Control == null)
			{
				SetNativeControl(new UITableView());
			}

			if (e.OldElement != null)
			{
				// unsubscribe
			}

			if (e.NewElement != null)
			{
				// subscribe

				var s = new NativeiOSListViewSource(e.NewElement);
				Control.Source = s;
			}
		}

		protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == NativeListView.ItemsProperty.PropertyName)
			{
				// update the Items list in the UITableViewSource
				var s = new NativeiOSListViewSource(Element);

				Control.Source = s;
			}
		}

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			base.GetDesiredSize(widthConstraint, heightConstraint);
			return Control.GetSizeRequest(widthConstraint, heightConstraint, 44, 44);
		}
	}

	public class NativeListViewRenderer : Handlers.Compatibility.ViewRenderer<NativeListView, UITableView>
	{
		public NativeListViewRenderer()
		{
		}

		protected override void OnElementChanged(ElementChangedEventArgs<NativeListView> e)
		{
			base.OnElementChanged(e);

			if (Control == null)
			{
				SetNativeControl(new UITableView());
			}

			if (e.OldElement != null)
			{
				// unsubscribe
			}

			if (e.NewElement != null)
			{
				// subscribe

				var s = new NativeListViewSource(e.NewElement);
				Control.Source = s;
			}
		}

		protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == NativeListView.ItemsProperty.PropertyName)
			{
				// update the Items list in the UITableViewSource
				var s = new NativeListViewSource(Element);
				Control.Source = s;
			}
		}

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return Control.GetSizeRequest(widthConstraint, heightConstraint, 44, 44);
		}
	}

	public class NativeiOSListViewSource : UITableViewSource
	{
		// declare vars
		IList<DataSource> _tableItems;
		NativeListView2 _listView;
		readonly NSString _cellIdentifier = new NSString("TableCell");

		public IEnumerable<DataSource> Items
		{
			//get{ }
			set { _tableItems = new List<DataSource>(value); }
		}

		public NativeiOSListViewSource(NativeListView2 view)
		{
			_tableItems = new List<DataSource>(view.Items);
			_listView = view;
		}

		/// <summary>
		/// Called by the TableView to determine how many cells to create for that particular section.
		/// </summary>

		public override nint RowsInSection(UITableView tableview, nint section)
		{
			return _tableItems.Count;
		}

		#region user interaction methods

		public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
		{
			_listView.NotifyItemSelected(_tableItems[indexPath.Row]);
			Console.WriteLine("Row " + indexPath.Row.ToString() + " selected");
			tableView.DeselectRow(indexPath, true);
		}

		public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
		{
			Console.WriteLine("Row " + indexPath.Row.ToString() + " deselected");
		}

		#endregion

		/// <summary>
		/// Called by the TableView to get the actual UITableViewCell to render for the particular section and row
		/// </summary>
		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{
			// request a recycled cell to save memory
			NativeiOSListViewCell cell = tableView.DequeueReusableCell(_cellIdentifier) as NativeiOSListViewCell;

			// if there are no cells to reuse, create a new one
			if (cell == null)
			{
				cell = new NativeiOSListViewCell(_cellIdentifier);
			}

			if (string.IsNullOrWhiteSpace(_tableItems[indexPath.Row].ImageFilename))
			{
				cell.UpdateCell(_tableItems[indexPath.Row].Name
					, _tableItems[indexPath.Row].Category
					, null);
			}
			else
			{
				cell.UpdateCell(_tableItems[indexPath.Row].Name
					, _tableItems[indexPath.Row].Category
					, UIImage.FromFile("Images/" + _tableItems[indexPath.Row].ImageFilename + ".jpg"));
			}

			return cell;
		}
	}

	public class NativeListViewSource : UITableViewSource
	{
		// declare vars
		IList<string> _tableItems;
		string _cellIdentifier = "TableCell";
		NativeListView _listView;

		public IEnumerable<string> Items
		{
			set
			{
				_tableItems = new List<string>(value);
			}
		}

		public NativeListViewSource(NativeListView view)
		{
			_tableItems = new List<string>(view.Items);
			_listView = view;
		}

		public override nint RowsInSection(UITableView tableview, nint section)
		{
			return _tableItems.Count;
		}

		#region user interaction methods

		public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
		{
			_listView.NotifyItemSelected(_tableItems[indexPath.Row]);

			Console.WriteLine("Row " + indexPath.Row.ToString() + " selected");

			tableView.DeselectRow(indexPath, true);
		}

		public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
		{
			Console.WriteLine("Row " + indexPath.Row.ToString() + " deselected");
		}

		#endregion

		/// <summary>
		/// Called by the TableView to get the actual UITableViewCell to render for the particular section and row
		/// </summary>
		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{
			// declare vars
			UITableViewCell cell = tableView.DequeueReusableCell(_cellIdentifier);
			//string item = tableItems [indexPath.Row]; //.Items[indexPath.Row];

			// if there are no cells to reuse, create a new one
			if (cell == null)
				cell = new UITableViewCell(UITableViewCellStyle.Subtitle, _cellIdentifier);

#pragma warning disable CA1416 // TODO: 'UITableViewCell.TextLabel' is unsupported on: 'ios' 14.0 and later
			// set the item text
#pragma warning disable CA1422 // Validate platform compatibility
			cell.TextLabel.Text = _tableItems[indexPath.Row]; //.Items[indexPath.Row].Heading;
#pragma warning restore CA1422 // Validate platform compatibility
#pragma warning restore CA1416

			// if it's a cell style that supports a subheading, set it
			//			if(item.CellStyle == UITableViewCellStyle.Subtitle 
			//				|| item.CellStyle == UITableViewCellStyle.Value1
			//				|| item.CellStyle == UITableViewCellStyle.Value2)
			//			{ cell.DetailTextLabel.Text = item.SubHeading; }

			// if the item has a valid image, and it's not the contact style (doesn't support images)
			//			if(!string.IsNullOrEmpty(item.ImageName) && item.CellStyle != UITableViewCellStyle.Value2)
			//			{
			//				if(File.Exists(item.ImageName))
			//					cell.ImageView.Image = UIImage.FromBundle(item.ImageName);
			//			}

			// set the accessory
			cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;

			return cell;
		}

	}

	[System.Obsolete]
	public class CustomContentRenderer : ViewRenderer
	{
	}

	public class CollectionViewRenderer : Controls.Handlers.Compatibility.ViewRenderer<Bugzilla21177.CollectionView, UICollectionView>
	{
		public void ItemSelected(UICollectionView collectionViewView, NSIndexPath indexPath)
		{
			Element.InvokeItemSelected(indexPath.Row);
		}

		CollectionViewController _controller;

		protected override void OnElementChanged(ElementChangedEventArgs<Bugzilla21177.CollectionView> e)
		{
			if (e.NewElement != null)
			{
				var flowLayout = new UICollectionViewFlowLayout
				{
					SectionInset = new UIEdgeInsets(20, 20, 20, 20),
					ScrollDirection = UICollectionViewScrollDirection.Vertical,
					MinimumInteritemSpacing = 5, // minimum spacing between cells 
					MinimumLineSpacing = 5 // minimum spacing between rows if ScrollDirection is Vertical or between columns if Horizontal 
				};
				_controller = new CollectionViewController(flowLayout, ItemSelected);
				SetNativeControl(_controller.CollectionView);
			}

			base.OnElementChanged(e);
		}
	}

	public class CollectionViewController : UICollectionViewController
	{
		readonly OnItemSelected _onItemSelected;
		static NSString cellId = new NSString("CollectionViewCell");
		List<string> items;

		public delegate void OnItemSelected(UICollectionView collectionView, NSIndexPath indexPath);

		public CollectionViewController(UICollectionViewLayout layout, OnItemSelected onItemSelected) : base(layout)
		{
			items = new List<string>();
			for (int i = 0; i < 20; i++)
			{
				items.Add($"#{i}");
			}
			_onItemSelected = onItemSelected;
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			CollectionView.RegisterClassForCell(typeof(CollectionViewCell), cellId);
		}

		public override nint NumberOfSections(UICollectionView collectionView)
		{
			return 1;
		}

		public override nint GetItemsCount(UICollectionView collectionView, nint section)
		{
			return items.Count;
		}

		public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
		{
			var cell = (CollectionViewCell)collectionView.DequeueReusableCell(cellId, indexPath);
			cell.Label.Text = items[indexPath.Row];
			return cell;
		}

		public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
		{
			_onItemSelected(collectionView, indexPath);
		}
	}

	public class CollectionViewCell : UICollectionViewCell
	{
		public UILabel Label { get; private set; }

		[Export("initWithFrame:")]
		public CollectionViewCell(RectangleF frame) : base(frame)
		{
			var rand = new Random();
			BackgroundView = new UIView { BackgroundColor = UIColor.FromRGB(rand.Next(0, 256), rand.Next(0, 256), rand.Next(0, 256)) };
			SelectedBackgroundView = new UIView { BackgroundColor = UIColor.Green };
			ContentView.Layer.BorderColor = UIColor.LightGray.CGColor;
			ContentView.Layer.BorderWidth = 2.0f;
			Label = new UILabel(frame);
			Label.Center = ContentView.Center;
			ContentView.AddSubview(Label);
		}
	}

	public class TabbedPageWithCustomBarColorRenderer : Handlers.Compatibility.TabbedRenderer
	{
		public TabbedPageWithCustomBarColorRenderer()
		{
			TabBar.TintColor = UIColor.White;
			TabBar.BarTintColor = UIColor.Purple;

			//UITabBar.Appearance.TintColor = UIColor.White;
			//UITabBar.Appearance.BarTintColor = UIColor.Purple;
		}
	}

	public class AccessoryViewCellRenderer : Handlers.Compatibility.ViewCellRenderer
	{
		public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
		{
			var cell = base.GetCell(item, reusableCell, tv);

			// remove highlight on selected cell
			cell.SelectionStyle = UITableViewCellSelectionStyle.None;

			// iOS right arrow
			cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;

			return cell;
		}
	}

	public class NoSelectionViewCellRenderer : Handlers.Compatibility.ViewCellRenderer
	{
		public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
		{
			var cell = base.GetCell(item, reusableCell, tv);

			// remove highlight on selected cell
			cell.SelectionStyle = UITableViewCellSelectionStyle.None;

			return cell;
		}
	}

	[System.Obsolete]
	public class EditorRendererKeyboardFlags : EditorRenderer
	{
		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
			KeyboardFlagExtensions.SetFlags(
				(value) => Control.AutocapitalizationType = value,
				(((Issue1683.EditorKeyboardFlags)Element).FlagsToTestFor)
			);

			Control.AutocapitalizationType.TestKeyboardFlags(((Issue1683.EditorKeyboardFlags)Element).FlagsToTestFor);
		}
	}

	[System.Obsolete]
	public class EntryRendererKeyboardFlags : EntryRenderer
	{
		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
			KeyboardFlagExtensions.SetFlags(
				(value) => Control.AutocapitalizationType = value,
				(((Issue1683.EntryKeyboardFlags)Element).FlagsToTestFor)
			);

			Control.AutocapitalizationType.TestKeyboardFlags(((Issue1683.EntryKeyboardFlags)Element).FlagsToTestFor);
		}
	}


	public static class KeyboardFlagExtensions
	{
		public static void SetFlags(Action<UITextAutocapitalizationType> setField, KeyboardFlags? flags)
		{
			if (flags == null)
			{
				return;
			}


			if (flags.Value.HasFlag(KeyboardFlags.CapitalizeSentence))
			{
				setField(UITextAutocapitalizationType.Sentences);
			}
			else if (flags.Value.HasFlag(KeyboardFlags.CapitalizeCharacter))
			{
				setField(UITextAutocapitalizationType.AllCharacters);
			}
			else if (flags.Value.HasFlag(KeyboardFlags.CapitalizeWord))
			{
				setField(UITextAutocapitalizationType.Words);
			}
		}

		public static void TestKeyboardFlags(this UITextAutocapitalizationType currentValue, KeyboardFlags? flags)
		{
			if (flags == null)
			{
				return;
			}

			if (flags.Value.HasFlag(KeyboardFlags.CapitalizeSentence))
			{
				if (currentValue != UITextAutocapitalizationType.Sentences)
				{
					throw new Exception("TextFlagCapSentences not correctly set");
				}
			}

			else if (flags.Value.HasFlag(KeyboardFlags.CapitalizeCharacter))
			{
				if (currentValue != UITextAutocapitalizationType.AllCharacters)
				{
					throw new Exception("CapitalizeCharacter not correctly set");
				}
			}
			else if (flags.Value.HasFlag(KeyboardFlags.CapitalizeWord))
			{

				if (currentValue != UITextAutocapitalizationType.Words)
				{
					throw new Exception("CapitalizeWord not correctly set");
				}
			}
		}
	}

	[System.Runtime.Versioning.UnsupportedOSPlatform("ios14.0")]
	[System.Runtime.Versioning.UnsupportedOSPlatform("tvos14.0")]
	public class ExtendedEntryCellRenderer : Handlers.Compatibility.EntryCellRenderer
	{
		public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
		{
			var entryCell = (EntryCell)item;
			var cell = base.GetCell(item, reusableCell, tv);
			if (cell != null && entryCell != null)
			{
				var tvc = cell as EntryCellTableViewCell;
				if (tvc != null)
				{
					// cell.TextField.thingstocallhere, for example:
					tvc.TextField.Text = entryCell.Text;
					tvc.TextField.TextColor = UIColor.Red;
				}
			}
			return cell;
		}
	}

}

