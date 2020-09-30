using System;

namespace Xamarin.Forms
{
	public class ImageCell : TextCell
	{
		public static readonly BindableProperty ImageSourceProperty = BindableProperty.Create("ImageSource", typeof(ImageSource), typeof(ImageCell), null,
			propertyChanging: (bindable, oldvalue, newvalue) => ((ImageCell)bindable).OnSourcePropertyChanging((ImageSource)oldvalue, (ImageSource)newvalue),
			propertyChanged: (bindable, oldvalue, newvalue) => ((ImageCell)bindable).OnSourcePropertyChanged((ImageSource)oldvalue, (ImageSource)newvalue));

		public ImageCell()
		{
			Disappearing += (sender, e) =>
			{
				if (ImageSource == null)
					return;
				ImageSource.Cancel();
			};
		}

		[TypeConverter(typeof(ImageSourceConverter))]
		public ImageSource ImageSource
		{
			get { return (ImageSource)GetValue(ImageSourceProperty); }
			set { SetValue(ImageSourceProperty, value); }
		}

		protected override void OnBindingContextChanged()
		{
			if (ImageSource != null)
				SetInheritedBindingContext(ImageSource, BindingContext);

			base.OnBindingContextChanged();
		}

		void OnSourceChanged(object sender, EventArgs eventArgs)
		{
			OnPropertyChanged(ImageSourceProperty.PropertyName);
		}

		void OnSourcePropertyChanged(ImageSource oldvalue, ImageSource newvalue)
		{
			if (newvalue != null)
			{
				newvalue.SourceChanged += OnSourceChanged;
				SetInheritedBindingContext(newvalue, BindingContext);
			}
		}

		void OnSourcePropertyChanging(ImageSource oldvalue, ImageSource newvalue)
		{
			if (oldvalue != null)
				oldvalue.SourceChanged -= OnSourceChanged;
		}
	}
}