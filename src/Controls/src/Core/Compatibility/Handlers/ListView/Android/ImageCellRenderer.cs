#nullable disable
using System.ComponentModel;
using Android.Content;
using Android.Views;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public class ImageCellRenderer : TextCellRenderer
	{
#pragma warning disable CS0618 // Type or member is obsolete
		protected override global::Android.Views.View GetCellCore(Cell item, global::Android.Views.View convertView, ViewGroup parent, Context context)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			var result = (BaseCellView)base.GetCellCore(item, convertView, parent, context);

			UpdateImage();
			UpdateFlowDirection();

			return result;
		}

		protected override void OnCellPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			base.OnCellPropertyChanged(sender, args);
#pragma warning disable CS0618 // Type or member is obsolete
			if (args.PropertyName == ImageCell.ImageSourceProperty.PropertyName)
				UpdateImage();
			else if (args.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateFlowDirection();
#pragma warning restore CS0618 // Type or member is obsolete
		}

		void UpdateImage()
		{
#pragma warning disable CS0618 // Type or member is obsolete
			var cell = (ImageCell)Cell;
#pragma warning restore CS0618 // Type or member is obsolete
			if (cell.ImageSource != null)
			{
				View.SetImageVisible(true);
				View.SetImageSource(cell.ImageSource);
			}
			else
				View.SetImageVisible(false);
		}

		void UpdateFlowDirection()
		{
			View.UpdateFlowDirection(ParentView);
		}
	}
}