#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ColumnDefinition.xml" path="Type[@FullName='Microsoft.Maui.Controls.ColumnDefinition']/Docs/*" />
	public sealed class ColumnDefinition : BindableObject, IDefinition, IGridColumnDefinition
	{
		/// <summary>Bindable property for <see cref="Width"/>.</summary>
		public static readonly BindableProperty WidthProperty = BindableProperty.Create(nameof(Width), typeof(GridLength), typeof(ColumnDefinition), GridLength.Star,
			propertyChanged: (bindable, oldValue, newValue) => ((ColumnDefinition)bindable).OnSizeChanged());

		/// <summary>Bindable property for <see cref="MinWidth"/>.</summary>
		public static readonly BindableProperty MinWidthProperty = BindableProperty.Create(
			nameof(MinWidth), typeof(double), typeof(ColumnDefinition), -1d,
			propertyChanged: (bindable, oldValue, newValue) => ((ColumnDefinition)bindable).OnSizeChanged());

		/// <summary>Bindable property for <see cref="MaxWidth"/>.</summary>
		public static readonly BindableProperty MaxWidthProperty = BindableProperty.Create(
			nameof(MaxWidth), typeof(double), typeof(ColumnDefinition), -1d,
			propertyChanged: (bindable, oldValue, newValue) => ((ColumnDefinition)bindable).OnSizeChanged());

		/// <include file="../../docs/Microsoft.Maui.Controls/ColumnDefinition.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public ColumnDefinition()
		{
		}

		public ColumnDefinition(GridLength width)
			=> SetValue(WidthProperty, width);

		/// <include file="../../docs/Microsoft.Maui.Controls/ColumnDefinition.xml" path="//Member[@MemberName='Width']/Docs/*" />
		[System.ComponentModel.TypeConverter(typeof(GridLengthTypeConverter))]
		public GridLength Width
		{
			get { return (GridLength)GetValue(WidthProperty); }
			set { SetValue(WidthProperty, value); }
		}

		/// <summary>
		/// Gets or sets the minimum allowed width of the column.
		/// </summary>
		public double MinWidth
		{
			get { return (double)GetValue(MinWidthProperty); }
			set { SetValue(MinWidthProperty, value); }
		}

		/// <summary>
		/// Gets or sets the maximum allowed width of the column.
		/// </summary>
		public double MaxWidth
		{
			get { return (double)GetValue(MaxWidthProperty); }
			set { SetValue(MaxWidthProperty, value); }
		}

		internal double ActualWidth { get; set; }

		internal double MinimumWidth { get; set; } = -1;

		public event EventHandler SizeChanged;

		void OnSizeChanged() => SizeChanged?.Invoke(this, EventArgs.Empty);
	}
}
