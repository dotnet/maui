using System;
using System.Linq;
using System.ComponentModel;
using AppKit;
using CoreGraphics;
using Xamarin.Forms.Platform.macOS.Extensions;

namespace Xamarin.Forms.Platform.MacOS
{
	internal class CellNSView : NSView, INativeElementView
	{
		static readonly NSColor s_defaultChildViewsBackground = NSColor.Clear;
		static readonly CGColor s_defaultHeaderViewsBackground = NSColor.LightGray.CGColor;
		Cell _cell;
		readonly NSTableViewCellStyle _style;

		public Action<object, PropertyChangedEventArgs> PropertyChanged;

		public CellNSView(NSTableViewCellStyle style)
		{
			WantsLayer = true;
			_style = style;
			CreateUI();
		}

		public NSTextField TextLabel { get; private set; }

		public NSTextField DetailTextLabel { get; private set; }

		public NSImageView ImageView { get; private set; }

		public NSView AccessoryView { get; private set; }

		public virtual Element Element => Cell;

		public Cell Cell
		{
			get { return _cell; }
			set
			{
				if (_cell == value)
					return;

				if (_cell != null)
					Device.BeginInvokeOnMainThread(_cell.SendDisappearing);

				_cell = value;

				if (_cell != null)
					Device.BeginInvokeOnMainThread(_cell.SendAppearing);
			}
		}

		public void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			PropertyChanged?.Invoke(this, e);
		}

		public override void Layout()
		{
			const int padding = 10;
			nfloat availableHeight = Frame.Height;
			nfloat availableWidth = Frame.Width - padding * 2;
			nfloat imageWidth = 0;
			nfloat accessoryViewWidth = 0;

			if (ImageView != null)
			{
				nfloat imageHeight = imageWidth = availableHeight;
				ImageView.Frame = new CGRect(padding, 0, imageWidth, imageHeight);
			}

			if (AccessoryView != null)
			{
				accessoryViewWidth = _style == NSTableViewCellStyle.Value1 ? 50 : availableWidth - 100;
				AccessoryView.Frame = new CGRect(availableWidth - accessoryViewWidth + padding, 0, accessoryViewWidth,
					availableHeight);
				foreach (var subView in AccessoryView.Subviews)
				{
					//try to find the size the control wants, if no width use default width
					var size = subView.FittingSize;
					if (size.Width == 0)
						size.Width = accessoryViewWidth;

					var x = AccessoryView.Bounds.Width - size.Width;
					var y = (AccessoryView.Bounds.Height - size.Height) / 2;
					subView.Frame = new CGRect(new CGPoint(x, y), size);
				}
			}

			nfloat labelHeights = availableHeight;
			nfloat labelWidth = availableWidth - imageWidth - accessoryViewWidth;

			if (DetailTextLabel != null)
			{

				if (!string.IsNullOrEmpty(DetailTextLabel?.StringValue))
				{
					labelHeights = availableHeight / 2;
					DetailTextLabel.CenterTextVertically(new CGRect(imageWidth + padding, 0, labelWidth, labelHeights));
				}
			}

			TextLabel?.CenterTextVertically(new CGRect(imageWidth + padding, availableHeight - labelHeights, labelWidth,
				labelHeights));

			base.Layout();
		}

		internal static NSView GetNativeCell(NSTableView tableView, Cell cell, string templateId = "", bool isHeader = false,
			bool isRecycle = false)
		{
			var reusable = tableView.MakeView(templateId, tableView);
			NSView nativeCell;
			if (reusable == null || !isRecycle)
			{
				var renderer = (CellRenderer)Internals.Registrar.Registered.GetHandlerForObject<IRegisterable>(cell);
				nativeCell = renderer.GetCell(cell, null, tableView);
			}
			else
			{
				nativeCell = reusable;
			}

			if (string.IsNullOrEmpty(nativeCell.Identifier))
				nativeCell.Identifier = templateId;

			if (!isHeader) return nativeCell;
			if (nativeCell.Layer != null) nativeCell.Layer.BackgroundColor = s_defaultHeaderViewsBackground;
			return nativeCell;
		}

		void CreateUI()
		{
			var style = _style;
			if (style != NSTableViewCellStyle.Empty)
			{
				AddSubview(TextLabel = new NSTextField
				{
					Bordered = false,
					Selectable = false,
					Editable = false,
					Font = NSFont.LabelFontOfSize(NSFont.SystemFontSize)
				});

				TextLabel.Cell.BackgroundColor = s_defaultChildViewsBackground;

				if (style == NSTableViewCellStyle.Image || style == NSTableViewCellStyle.Subtitle ||
					style == NSTableViewCellStyle.ImageSubtitle)
				{
					AddSubview(DetailTextLabel = new NSTextField
					{
						Bordered = false,
						Selectable = false,
						Editable = false,
						Font = NSFont.LabelFontOfSize(NSFont.SmallSystemFontSize)
					});
					DetailTextLabel.Cell.BackgroundColor = s_defaultChildViewsBackground;
				}

				if (style == NSTableViewCellStyle.Image || style == NSTableViewCellStyle.ImageSubtitle)
					AddSubview(ImageView = new NSImageView());

				if (style == NSTableViewCellStyle.Value1 || style == NSTableViewCellStyle.Value2)
				{
					var accessoryView = new NSView { WantsLayer = true };
					accessoryView.Layer.BackgroundColor = s_defaultChildViewsBackground.CGColor;
					AddSubview(AccessoryView = accessoryView);
				}
			}
		}
		public override void RightMouseDown(NSEvent theEvent)
		{
			HandleContextActions(theEvent);

			base.RightMouseDown(theEvent);
		}

		void HandleContextActions(NSEvent theEvent)
		{
			var contextActionCell = this.Element as Cell;
			var contextActionsCount = contextActionCell.ContextActions.Count;
			if (contextActionsCount > 0)
			{
				NSMenu menu = new NSMenu();
				for (int i = 0; i < contextActionsCount; i++)
				{
					var contextAction = contextActionCell.ContextActions[i];
					var nsMenuItem = contextAction.ToNSMenuItem(i);
					menu.AddItem(nsMenuItem);
				}

				NSMenu.PopUpContextMenu(menu, theEvent, this);
			}
		}
	}
}