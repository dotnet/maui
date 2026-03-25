#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>Defines the height of a row in a <see cref="Grid"/>.</summary>
	public sealed class RowDefinition : BindableObject, IDefinition, IGridRowDefinition
	{
		/// <summary>Bindable property for <see cref="Height"/>.</summary>
		public static readonly BindableProperty HeightProperty = BindableProperty.Create(nameof(Height), typeof(GridLength), typeof(RowDefinition), GridLength.Star,
			propertyChanged: (bindable, oldValue, newValue) => ((RowDefinition)bindable).OnSizeChanged());

		/// <summary>Initializes a new <see cref="RowDefinition"/> with default height (<see cref="GridLength.Star"/>).</summary>
		public RowDefinition()
		{
		}

		public RowDefinition(GridLength height)
		{
			SetValue(HeightProperty, height);
		}

		/// <summary>Gets or sets the height of the row. This is a bindable property.</summary>
		[System.ComponentModel.TypeConverter(typeof(Converters.GridLengthTypeConverter))]
		public GridLength Height
		{
			get { return (GridLength)GetValue(HeightProperty); }
			set { SetValue(HeightProperty, value); }
		}

		internal double ActualHeight { get; set; }

		internal double MinimumHeight { get; set; } = -1;

		public event EventHandler SizeChanged;

		void OnSizeChanged() => SizeChanged?.Invoke(this, EventArgs.Empty);
	}
}
