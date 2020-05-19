using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
{
	public class ProductCellView : StackLayout
	{
		Label _timeLabel;
		Label _brandLabel;
		StackLayout _stack;
		public ProductCellView (string text)
		{
			_stack = new StackLayout ();
			_brandLabel = new Label {Text = "BrandLabel", HorizontalTextAlignment = TextAlignment.Center};
			_stack.Children.Add (_brandLabel);


			var frame = new Frame {
				Content = _stack,
				BackgroundColor = new[] { Device.Android, Device.UWP }.Contains(Device.RuntimePlatform) ? new Color(0.2) : new Color(1)
			};
			_timeLabel = new Label {
				Text = text
			};
			Children.Add (_timeLabel);
			Children.Add (frame);
			Padding = new Size (20, 20);
		}
	}
}
