using System.ComponentModel;
using System.Threading.Tasks;
using AppKit;
using Foundation;

namespace Xamarin.Forms.Platform.MacOS
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

		protected override async void HandlePropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			var tvc = (CellNSView)sender;
			var imageCell = (ImageCell)tvc.Cell;

			base.HandlePropertyChanged(sender, args);

			if (args.PropertyName == ImageCell.ImageSourceProperty.PropertyName)
				await SetImage(imageCell, tvc);
		}

		static async Task SetImage(ImageCell cell, CellNSView target)
		{
			var source = cell.ImageSource;

			target.ImageView.Image = null;

			IImageSourceHandler handler;

			if (source != null && (handler = Internals.Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(source)) != null)
			{
				NSImage uiimage;
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
					target.NeedsLayout = true;
				});
			}
			else
				target.ImageView.Image = null;
		}
	}
}