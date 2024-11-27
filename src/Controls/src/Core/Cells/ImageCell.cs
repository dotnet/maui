#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/ImageCell.xml" path="Type[@FullName='Microsoft.Maui.Controls.ImageCell']/Docs/*" />
	public class ImageCell : TextCell
	{
		/// <summary>Bindable property for <see cref="ImageSource"/>.</summary>
		public static readonly BindableProperty ImageSourceProperty = BindableProperty.Create(nameof(ImageSource), typeof(ImageSource), typeof(ImageCell), null,
			propertyChanging: (bindable, oldvalue, newvalue) => ((ImageCell)bindable).OnSourcePropertyChanging((ImageSource)oldvalue, (ImageSource)newvalue),
			propertyChanged: (bindable, oldvalue, newvalue) => ((ImageCell)bindable).OnSourcePropertyChanged((ImageSource)oldvalue, (ImageSource)newvalue));

		/// <include file="../../../docs/Microsoft.Maui.Controls/ImageCell.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public ImageCell()
		{
			Disappearing += (sender, e) =>
			{
				if (ImageSource == null)
					return;
				ImageSource.Cancel();
			};
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/ImageCell.xml" path="//Member[@MemberName='ImageSource']/Docs/*" />
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