using System.ComponentModel;
using Android.Content;
using Android.Views;

namespace Xamarin.Forms.Platform.Android
{
	public class ImageCellRenderer : TextCellRenderer
	{
		protected override global::Android.Views.View GetCellCore(Cell item, global::Android.Views.View convertView, ViewGroup parent, Context context)
		{
			var result = (BaseCellView)base.GetCellCore(item, convertView, parent, context);

			UpdateImage();

			return result;
		}

		protected override void OnCellPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			base.OnCellPropertyChanged(sender, args);
			if (args.PropertyName == ImageCell.ImageSourceProperty.PropertyName)
				UpdateImage();
		}

		void UpdateImage()
		{
			var cell = (ImageCell)Cell;
			if (cell.ImageSource != null)
			{
				View.SetImageVisible(true);
				View.SetImageSource(cell.ImageSource);
			}
			else
				View.SetImageVisible(false);
		}
	}
}