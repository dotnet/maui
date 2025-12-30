#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>An <see cref="Microsoft.Maui.Controls.IDefinition"/> that defines properties for a column in a <see cref="Microsoft.Maui.Controls.Grid"/>.</summary>
	public sealed class ColumnDefinition : BindableObject, IDefinition, IGridColumnDefinition
	{
		/// <summary>Bindable property for <see cref="Width"/>.</summary>
		public static readonly BindableProperty WidthProperty = BindableProperty.Create(nameof(Width), typeof(GridLength), typeof(ColumnDefinition), GridLength.Star,
			propertyChanged: (bindable, oldValue, newValue) => ((ColumnDefinition)bindable).OnSizeChanged());

		/// <summary>Creates a new <see cref="Microsoft.Maui.Controls.ColumnDefinition"/> object with default values.</summary>
		public ColumnDefinition()
		{
		}

		public ColumnDefinition(GridLength width)
			=> SetValue(WidthProperty, width);

		/// <summary>Gets or sets the width of the column.</summary>
		[System.ComponentModel.TypeConverter(typeof(Converters.GridLengthTypeConverter))]
		public GridLength Width
		{
			get { return (GridLength)GetValue(WidthProperty); }
			set { SetValue(WidthProperty, value); }
		}

		internal double ActualWidth { get; set; }

		internal double MinimumWidth { get; set; } = -1;

		public event EventHandler SizeChanged;

		void OnSizeChanged() => SizeChanged?.Invoke(this, EventArgs.Empty);
	}
}
