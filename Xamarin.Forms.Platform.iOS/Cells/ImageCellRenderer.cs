using System.ComponentModel;
using System.Threading.Tasks;
using Foundation;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class ImageCellRenderer : TextCellRenderer
	{
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

			target.ImageView.Image = null;

			IImageSourceHandler handler;

			if (source != null && (handler = Internals.Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(source)) != null)
			{
				UIImage uiimage;
				try
				{
					uiimage = await handler.LoadImageAsync(source).ConfigureAwait(false);
				}
				catch (TaskCanceledException)
				{
					uiimage = null;
				}

				NSRunLoop.Main.BeginInvokeOnMainThread(() =>
				{
					if (target.Cell != null)
					{
						target.ImageView.Image = uiimage;
						target.SetNeedsLayout();
					}
					else
						uiimage?.Dispose();
				});
			}
			else
				target.ImageView.Image = null;
		}
	}
}
