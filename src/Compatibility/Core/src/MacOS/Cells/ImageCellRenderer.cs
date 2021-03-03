using System.ComponentModel;
using System.Threading.Tasks;
using AppKit;
using Foundation;

namespace Microsoft.Maui.Controls.Compatibility.Platform.MacOS
{
	public class ImageCellRenderer : TextCellRenderer
	{
		public override NSView GetCell(Cell item, NSView reusableView, NSTableView tv)
		{
			var tvc = reusableView as CellNSView ?? new CellNSView(NSTableViewCellStyle.ImageSubtitle);

			var result = (CellNSView)base.GetCell(item, tvc, tv);

			var imageCell = (ImageCell)item;

			WireUpForceUpdateSizeRequested(item, result, tv);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			SetImage(imageCell, result);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

			return result;
		}

		protected override void HandlePropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			var tvc = (CellNSView)sender;
			var imageCell = (ImageCell)tvc.Cell;

			base.HandlePropertyChanged(sender, args);

			if (args.PropertyName == ImageCell.ImageSourceProperty.PropertyName)
				SetImage(imageCell, tvc);
		}

		static void SetImage(ImageCell cell, CellNSView target)
		{
			target.ImageView.Image = null;

			_ = cell.ApplyNativeImageAsync(ImageCell.ImageSourceProperty, image =>
			{
				target.ImageView.Image = image;
				target.NeedsLayout = true;
			});
		}
	}
}