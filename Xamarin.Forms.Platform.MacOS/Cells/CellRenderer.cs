using System;
using AppKit;
using CoreGraphics;

namespace Xamarin.Forms.Platform.MacOS
{
	public class CellRenderer : IRegisterable
	{
		static readonly BindableProperty s_realCellProperty = BindableProperty.CreateAttached("RealCell", typeof(NSView),
			typeof(Cell), null);

		EventHandler _onForceUpdateSizeRequested;

		public virtual NSView GetCell(Cell item, NSView reusableView, NSTableView tv)
		{
			var tvc = reusableView as CellNSView ?? new CellNSView(NSTableViewCellStyle.Default);

			tvc.Cell = item;

			WireUpForceUpdateSizeRequested(item, tvc, tv);

			tvc.TextLabel.StringValue = item.ToString();

			UpdateBackground(tvc, item);

			return tvc;
		}

		protected void UpdateBackground(NSView tableViewCell, Cell cell)
		{
			tableViewCell.WantsLayer = true;
			var bgColor = NSColor.White;
			var element = cell.RealParent as VisualElement;
			if (element != null)
				bgColor = element.BackgroundColor == Color.Default ? bgColor : element.BackgroundColor.ToNSColor();

			UpdateBackgroundChild(cell, bgColor);

			tableViewCell.Layer.BackgroundColor = bgColor.CGColor;
		}

		protected void WireUpForceUpdateSizeRequested(ICellController cell, NSView nativeCell, NSTableView tableView)
		{
			cell.ForceUpdateSizeRequested -= _onForceUpdateSizeRequested;

			_onForceUpdateSizeRequested = (sender, e) =>
			{
				//TODO: Implement ForceUpdateSize
			};

			cell.ForceUpdateSizeRequested += _onForceUpdateSizeRequested;
		}

		internal virtual void UpdateBackgroundChild(Cell cell, NSColor backgroundColor)
		{
		}

		internal static NSView GetRealCell(BindableObject cell)
		{
			return (NSView)cell.GetValue(s_realCellProperty);
		}

		internal static void SetRealCell(BindableObject cell, NSView renderer)
		{
			cell.SetValue(s_realCellProperty, renderer);
		}
	}
}