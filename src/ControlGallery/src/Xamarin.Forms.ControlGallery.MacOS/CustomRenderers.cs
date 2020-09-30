using System;
using System.Collections.Generic;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.MacOS;
using Xamarin.Forms.Controls.Issues;
using Xamarin.Forms.Platform.MacOS;

[assembly: ExportRenderer(typeof(NativeCell), typeof(NativeMacCellRenderer))]
[assembly: ExportRenderer(typeof(NativeListView2), typeof(NativeMacOSListViewRenderer))]
[assembly: ExportRenderer(typeof(NativeListView), typeof(NativeListViewRenderer))]
namespace Xamarin.Forms.ControlGallery.MacOS
{
	public class NativeMacOSListViewRenderer : ViewRenderer<NativeListView2, NSView>
	{
		NSTableView _nsTableView;
		public NativeMacOSListViewRenderer()
		{
		}

		protected override void OnElementChanged(ElementChangedEventArgs<NativeListView2> e)
		{
			base.OnElementChanged(e);

			if (Control == null)
			{
				var scroller = new NSScrollView
				{
					AutoresizingMask = NSViewResizingMask.HeightSizable | NSViewResizingMask.WidthSizable,
					DocumentView = _nsTableView = new NSTableView().AsListViewLook()
				};

				_nsTableView.RowHeight = 60;
				SetNativeControl(scroller);
			}

			if (e.OldElement != null)
			{
				// unsubscribe
			}

			if (e.NewElement != null)
			{
				// subscribe

				var s = new NativeiOSListViewSource(e.NewElement, _nsTableView);
				_nsTableView.Source = s;
			}
		}

		protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == NativeListView.ItemsProperty.PropertyName)
			{
				// update the Items list in the UITableViewSource
				var s = new NativeiOSListViewSource(Element, _nsTableView);
				_nsTableView.Source = s;
			}
		}

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return Control.GetSizeRequest(widthConstraint, heightConstraint, 44, 44);
		}
	}

	public class NativeListViewRenderer : ViewRenderer<NativeListView, NSView>
	{
		public NativeListViewRenderer()
		{
		}
		NSTableView table;
		protected override void OnElementChanged(ElementChangedEventArgs<NativeListView> e)
		{
			base.OnElementChanged(e);

			if (Control == null)
			{
				var scroller = new NSScrollView
				{
					AutoresizingMask = NSViewResizingMask.HeightSizable | NSViewResizingMask.WidthSizable,
					DocumentView = table = new NSTableView().AsListViewLook()
				};

				table.RowHeight = 60;

				SetNativeControl(scroller);
			}

			if (e.OldElement != null)
			{
				// unsubscribe
			}

			if (e.NewElement != null)
			{
				// subscribe

				var s = new NativeListViewSource(e.NewElement, table);
				table.Source = s;
			}
		}

		protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == NativeListView.ItemsProperty.PropertyName)
			{
				// update the Items list in the UITableViewSource
				var s = new NativeListViewSource(Element, table);
				table.Source = s;
			}
		}

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return Control.GetSizeRequest(widthConstraint, heightConstraint, 44, 44);
		}
	}

	public class NativeiOSListViewSource : NSTableViewSource
	{
		IList<DataSource> _tableItems;
		NativeListView2 _listView;
		readonly NSTableView _nsTableView;
		readonly NSString _cellIdentifier = new NSString("TableCell");

		public IEnumerable<DataSource> Items
		{
			set { _tableItems = new List<DataSource>(value); }
		}

		public NativeiOSListViewSource(NativeListView2 view, NSTableView nsTableView)
		{
			_tableItems = new List<DataSource>(view.Items);
			_listView = view;
			_nsTableView = nsTableView;
		}

		public override nint GetRowCount(NSTableView tableView)
		{
			return _tableItems.Count;
		}

		public override void SelectionDidChange(NSNotification notification)
		{
			var selectedRow = (int)_nsTableView.SelectedRow;
			if (selectedRow == -1)
				return;
			_listView.NotifyItemSelected(_tableItems[selectedRow]);
			Console.WriteLine("Row " + selectedRow.ToString() + " selected");
			_nsTableView.DeselectRow(selectedRow);
		}

		public override NSView GetViewForItem(NSTableView tableView, NSTableColumn tableColumn, nint row)
		{
			NativeMacOsCell cell = tableView.MakeView(_cellIdentifier, tableView) as NativeMacOsCell;

			if (cell == null)
			{
				cell = new NativeMacOsCell(_cellIdentifier);
			}
			int rowNumber = (int)row;
			if (string.IsNullOrWhiteSpace(_tableItems[rowNumber].ImageFilename))
			{
				cell.UpdateCell(_tableItems[rowNumber].Name
					, _tableItems[rowNumber].Category
					, null);
			}
			else
			{
				cell.UpdateCell(_tableItems[rowNumber].Name
					, _tableItems[rowNumber].Category
					, new NSImage("Images/" + _tableItems[rowNumber].ImageFilename + ".jpg"));
			}

			return cell;
		}
	}

	public class NativeListViewSource : NSTableViewSource
	{
		// declare vars
		IList<string> _tableItems;
		string _cellIdentifier = "TableCell";
		NativeListView _listView;
		readonly NSTableView _nsTableView;

		public IEnumerable<string> Items
		{
			set
			{
				_tableItems = new List<string>(value);
			}
		}

		public NativeListViewSource(NativeListView view, NSTableView nsTableView)
		{
			_tableItems = new List<string>(view.Items);
			_listView = view;
			_nsTableView = nsTableView;
		}

		public override nint GetRowCount(NSTableView tableView)
		{
			return _tableItems.Count;
		}

		public override void SelectionDidChange(NSNotification notification)
		{
			var selectedRow = (int)_nsTableView.SelectedRow;
			if (selectedRow == -1)
				return;
			_listView.NotifyItemSelected(_tableItems[selectedRow]);
			Console.WriteLine("Row " + selectedRow.ToString() + " selected");
			_nsTableView.DeselectRow(selectedRow);
		}

		public override NSView GetViewForItem(NSTableView tableView, NSTableColumn tableColumn, nint row)
		{
			var cell = tableView.MakeView(_cellIdentifier, tableView);

			if (cell == null)
			{
				cell = new NSView(new CGRect(0, 0, tableView.Frame.Width, tableView.RowHeight));
				var textLabel = new NSTextField(new CGRect(1, 1, tableView.Frame.Width, tableView.RowHeight - 10));
				cell.AddSubview(textLabel);
			}
			var label = cell.Subviews[0] as NSTextField;
			label.StringValue = _tableItems[(int)row];
			return cell;
		}
	}

	public class NativeMacCellRenderer : ViewCellRenderer
	{
		static NSString s_rid = new NSString("NativeCell");

		public NativeMacCellRenderer()
		{
		}

		public override NSView GetCell(Cell item, NSView reusableView, NSTableView tv)
		{
			var x = (NativeCell)item;
			Console.WriteLine(x);

			NativeMacOsCell c = reusableView as NativeMacOsCell;

			if (c == null)
			{
				c = new NativeMacOsCell(s_rid);
			}

			NSImage i = null;
			if (!string.IsNullOrWhiteSpace(x.ImageFilename))
			{
				i = new NSImage("Images/" + x.ImageFilename + ".jpg");
			}

			base.WireUpForceUpdateSizeRequested(item, c, tv);

			c.UpdateCell(x.Name, x.Category, i);

			return c;
		}
	}

	public class NativeMacOsCell : NSView
	{
		NSTextField _headingLabel;
		NSTextField _subheadingLabel;
		NSImageView _imageView;

		public NativeMacOsCell() : this(new NSString("NativeMacOsCell"))
		{
		}
		public NativeMacOsCell(NSString cellId)
		{
			Identifier = cellId;
			WantsLayer = true;
			Layer.BackgroundColor = NSColor.FromRgb(218, 255, 127).CGColor;

			_imageView = new NSImageView();

			_headingLabel = new NSTextField()
			{
				Font = NSFont.FromFontName("Cochin-BoldItalic", 22f),
				TextColor = NSColor.FromRgb(127, 51, 0),
				BackgroundColor = NSColor.Clear
			};

			_subheadingLabel = new NSTextField()
			{
				Font = NSFont.FromFontName("AmericanTypewriter", 12f),
				TextColor = NSColor.FromRgb(38, 127, 0),
				Alignment = NSTextAlignment.Center,
				BackgroundColor = NSColor.Clear
			};

			AddSubview(_headingLabel);
			AddSubview(_subheadingLabel);
			AddSubview(_imageView);
		}

		public void UpdateCell(string caption, string subtitle, NSImage image)
		{
			_imageView.Image = image;
			_headingLabel.StringValue = caption;
			_subheadingLabel.StringValue = subtitle;
		}

		public override void Layout()
		{
			base.Layout();

			_imageView.Frame = new CGRect(Bounds.Width - 63, 5, 33, 33);
			_headingLabel.Frame = new CGRect(5, 4, Bounds.Width - 63, 25);
			_subheadingLabel.Frame = new CGRect(100, 18, 100, 20);
		}
	}

	public static class NSTableViewExtensions
	{
		public static NSTableView AsListViewLook(this NSTableView self)
		{
			self.SelectionHighlightStyle = NSTableViewSelectionHighlightStyle.SourceList;

			self.AllowsColumnReordering = false;
			self.AllowsColumnResizing = false;
			self.AllowsColumnSelection = false;

			//this is needed .. can we go around it ?
			self.AddColumn(new NSTableColumn("1"));
			//this line hides the header by default
			self.HeaderView = null;
			return self;
		}
	}

}
