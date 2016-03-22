using System;

namespace Xamarin.Forms
{
	public sealed class ColumnDefinition : BindableObject, IDefinition
	{
		public static readonly BindableProperty WidthProperty = BindableProperty.Create("Width", typeof(GridLength), typeof(ColumnDefinition), new GridLength(1, GridUnitType.Star),
			propertyChanged: (bindable, oldValue, newValue) => ((ColumnDefinition)bindable).OnSizeChanged());

		public ColumnDefinition()
		{
			MinimumWidth = -1;
		}

		public GridLength Width
		{
			get { return (GridLength)GetValue(WidthProperty); }
			set { SetValue(WidthProperty, value); }
		}

		internal double ActualWidth { get; set; }

		internal double MinimumWidth { get; set; }

		public event EventHandler SizeChanged;

		void OnSizeChanged()
		{
			EventHandler eh = SizeChanged;
			if (eh != null)
				eh(this, EventArgs.Empty);
		}
	}
}