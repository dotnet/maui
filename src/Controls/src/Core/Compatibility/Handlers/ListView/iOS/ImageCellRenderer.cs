#nullable disable
using System.ComponentModel;
using System.Threading.Tasks;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public class ImageCellRenderer : TextCellRenderer
	{
		[Preserve(Conditional = true)]
		public ImageCellRenderer()
		{
		}

#pragma warning disable CS0618 // Type or member is obsolete
		public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			var result = (CellTableViewCell)base.GetCell(item, reusableCell, tv);

#pragma warning disable CS0618 // Type or member is obsolete
			var imageCell = (ImageCell)item;
#pragma warning restore CS0618 // Type or member is obsolete

			SetImage(imageCell, result);

			return result;
		}

		protected override void HandleCellPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			var imageCell = (ImageCell)sender;
#pragma warning restore CS0618 // Type or member is obsolete
			var tvc = (CellTableViewCell)GetRealCell(imageCell);

			base.HandleCellPropertyChanged(sender, args);

#pragma warning disable CS0618 // Type or member is obsolete
			if (args.PropertyName == ImageCell.ImageSourceProperty.PropertyName)
				SetImage(imageCell, tvc);
#pragma warning restore CS0618 // Type or member is obsolete
		}

#pragma warning disable CS0618 // Type or member is obsolete
		void SetImage(ImageCell cell, CellTableViewCell target)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			var source = cell.ImageSource;

#pragma warning disable CA1416, CA1422 // TODO: 'UITableViewCell.ImageView' is unsupported on: 'ios' 14.0 and later
			target.ImageView.Image = null;

			source.LoadImage(cell.FindMauiContext(), (result) =>
			{
				var uiimage = result?.Value;
				if (uiimage is not null)
				{
					NSRunLoop.Main.BeginInvokeOnMainThread(() =>
					{
						if (target.Cell is not null)
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
			});
#pragma warning restore CA1416, CA1422
		}
	}
}
