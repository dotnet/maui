using System;

namespace Xamarin.Forms
{
	public sealed class RowDefinition : BindableObject, IDefinition
	{
		public static readonly BindableProperty HeightProperty = BindableProperty.Create("Height", typeof(GridLength), typeof(RowDefinition), new GridLength(1, GridUnitType.Star),
			propertyChanged: (bindable, oldValue, newValue) => ((RowDefinition)bindable).OnSizeChanged());

		public RowDefinition()
		{
			MinimumHeight = -1;
		}

		public GridLength Height
		{
			get { return (GridLength)GetValue(HeightProperty); }
			set { SetValue(HeightProperty, value); }
		}

		internal double ActualHeight { get; set; }

		internal double MinimumHeight { get; set; }

		public event EventHandler SizeChanged;

		void OnSizeChanged()
		{
			EventHandler eh = SizeChanged;
			if (eh != null)
				eh(this, EventArgs.Empty);
		}
	}
}