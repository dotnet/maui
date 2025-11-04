#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>A <see cref="Microsoft.Maui.Controls.TextCell"/> that has an image.</summary>
	[Obsolete("The controls which use ImageCell (ListView and TableView) are obsolete. Please use CollectionView instead.")]
	public class ImageCell : TextCell
	{
		/// <summary>Bindable property for <see cref="ImageSource"/>.</summary>
		public static readonly BindableProperty ImageSourceProperty = BindableProperty.Create(nameof(ImageSource), typeof(ImageSource), typeof(ImageCell), null,
			propertyChanging: (bindable, oldvalue, newvalue) => ((ImageCell)bindable).OnSourcePropertyChanging((ImageSource)oldvalue, (ImageSource)newvalue),
			propertyChanged: (bindable, oldvalue, newvalue) => ((ImageCell)bindable).OnSourcePropertyChanged((ImageSource)oldvalue, (ImageSource)newvalue));

		/// <summary>Initializes a new instance of the ImageCell class.</summary>
		public ImageCell()
		{
			Disappearing += (sender, e) =>
			{
				if (ImageSource == null)
					return;
				ImageSource.Cancel();
			};
		}

		/// <summary>Gets or sets the ImageSource from which the Image is loaded. This is a bindable property.</summary>
		[System.ComponentModel.TypeConverter(typeof(ImageSourceConverter))]
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