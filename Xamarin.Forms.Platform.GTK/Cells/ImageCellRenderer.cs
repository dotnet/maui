using System;
using System.ComponentModel;
using Xamarin.Forms.Platform.GTK.Extensions;

namespace Xamarin.Forms.Platform.GTK.Cells
{
    public class ImageCellRenderer : CellRenderer
    {
        public override CellBase GetCell(Cell item, Gtk.Container reusableView, Controls.ListView listView)
        {
            var gtkImageCell = base.GetCell(item, reusableView, listView) as ImageCell;
            var imageCell = (Xamarin.Forms.ImageCell)item;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            SetImage(imageCell, gtkImageCell);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            return gtkImageCell;
        }

        protected override Gtk.Container GetCellWidgetInstance(Cell item)
        {
            var imageCell = (Xamarin.Forms.ImageCell)item;

            var text = imageCell.Text ?? string.Empty;
            var textColor = imageCell.TextColor.ToGtkColor();
            var detail = imageCell.Detail ?? string.Empty;
            var detailColor = imageCell.DetailColor.ToGtkColor();

            return new ImageCell(
                    null,
                    text,
                    textColor,
                    detail,
                    detailColor);
        }

        protected override async void CellPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            base.CellPropertyChanged(sender, args);

            var gtkImageCell = (ImageCell)sender;
            var imageCell = (Xamarin.Forms.ImageCell)gtkImageCell.Cell;

            if (args.PropertyName == Xamarin.Forms.TextCell.TextProperty.PropertyName)
            {
                gtkImageCell.Text = imageCell.Text ?? string.Empty;
            }
            else if (args.PropertyName == Xamarin.Forms.TextCell.DetailProperty.PropertyName)
            {
                gtkImageCell.Detail = imageCell.Detail ?? string.Empty;
            }
            else if (args.PropertyName == Xamarin.Forms.ImageCell.ImageSourceProperty.PropertyName)
            {
                await SetImage(imageCell, gtkImageCell);
            }
        }

        private static async System.Threading.Tasks.Task SetImage(Xamarin.Forms.ImageCell cell, ImageCell target)
        {
            var source = cell.ImageSource;

            target.Image = null;

            Renderers.IImageSourceHandler handler;

            if (source != null && (handler =
                Internals.Registrar.Registered.GetHandlerForObject<Renderers.IImageSourceHandler>(source)) != null)
            {
                Gdk.Pixbuf image;

                try
                {
                    image = await handler.LoadImageAsync(source).ConfigureAwait(false);
                }
                catch (System.Threading.Tasks.TaskCanceledException)
                {
                    image = null;
                }
                catch(Exception)
                {
                    image = null;
                }

                target.Image = image;

            }
            else
                target.Image = null;
        }
    }
}