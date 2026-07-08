using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	[Obsolete("Use Microsoft.Maui.Controls.Platform.Compatibility.ImageCellRenderer instead")]
	public class ImageCellRenderer : TextCellRenderer
	{
		[Preserve(Conditional = true)]
		public ImageCellRenderer()
		{
		}

		public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
		{
			var result = (CellTableViewCell)base.GetCell(item, reusableCell, tv);

			var imageCell = (ImageCell)item;

			WireUpForceUpdateSizeRequested(item, result, tv);

			SetImage(imageCell, result);

			return result;
		}

		protected override void HandleCellPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			var imageCell = (ImageCell)sender;
			var tvc = (CellTableViewCell)GetRealCell(imageCell);

			base.HandleCellPropertyChanged(sender, args);

			if (args.PropertyName == ImageCell.ImageSourceProperty.PropertyName)
				SetImage(imageCell, tvc);
		}

		async void SetImage(ImageCell cell, CellTableViewCell target)
		{
			var source = cell.ImageSource;

#pragma warning disable CA1416, CA1422 // TODO: 'UITableViewCell.ImageView' is unsupported on: 'ios' 14.0 and later
			target.ImageView.Image = null;

			var uiimage = await source.GetNativeImageAsync().ConfigureAwait(false);
			if (uiimage != null)
			{
				NSRunLoop.Main.BeginInvokeOnMainThread(() =>
				{
					if (target.Cell != null)
					{
						target.ImageView.Image = uiimage;
						target.SetNeedsLayout();
					}
					else
					{
						uiimage?.Dispose();
					}
				});
			}
#pragma warning restore CA1416, CA1422
		}
	}
}
