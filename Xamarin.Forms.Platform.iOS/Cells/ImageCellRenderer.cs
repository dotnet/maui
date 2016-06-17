using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;
#if __UNIFIED__
using UIKit;
using Foundation;

#else
using MonoTouch.UIKit;
using MonoTouch.Foundation;
#endif

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

		protected override void HandlePropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			var tvc = (CellTableViewCell)sender;
			var imageCell = (ImageCell)tvc.Cell;

			base.HandlePropertyChanged(sender, args);

			if (args.PropertyName == ImageCell.ImageSourceProperty.PropertyName)
				SetImage(imageCell, tvc);
		}

		async void SetImage(ImageCell cell, CellTableViewCell target)
		{
			var source = cell.ImageSource;

			target.ImageView.Image = null;

			IImageSourceHandler handler;

			if (source != null && (handler = Registrar.Registered.GetHandler<IImageSourceHandler>(source.GetType())) != null)
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
					target.ImageView.Image = uiimage;
					target.SetNeedsLayout();
				});
			}
			else
				target.ImageView.Image = null;
		}
	}
}