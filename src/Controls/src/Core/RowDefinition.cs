#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/RowDefinition.xml" path="Type[@FullName='Microsoft.Maui.Controls.RowDefinition']/Docs/*" />
	public sealed class RowDefinition : BindableObject, IDefinition, IGridRowDefinition
	{
		/// <summary>Bindable property for <see cref="Height"/>.</summary>
		public static readonly BindableProperty HeightProperty = BindableProperty.Create(nameof(Height), typeof(GridLength), typeof(RowDefinition), GridLength.Star,
			propertyChanged: (bindable, oldValue, newValue) => ((RowDefinition)bindable).OnSizeChanged());

		/// <summary>Bindable property for <see cref="MinHeight"/>.</summary>
		public static readonly BindableProperty MinHeightProperty = BindableProperty.Create(
			nameof(MinHeight), typeof(double), typeof(RowDefinition), -1d,
			propertyChanged: (bindable, oldValue, newValue) => ((RowDefinition)bindable).OnSizeChanged());

		/// <summary>Bindable property for <see cref="MaxHeight"/>.</summary>
		public static readonly BindableProperty MaxHeightProperty = BindableProperty.Create(
			nameof(MaxHeight), typeof(double), typeof(RowDefinition), -1d,
			propertyChanged: (bindable, oldValue, newValue) => ((RowDefinition)bindable).OnSizeChanged());

		/// <include file="../../docs/Microsoft.Maui.Controls/RowDefinition.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public RowDefinition()
		{
		}

		public RowDefinition(GridLength height)
		{
			SetValue(HeightProperty, height);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/RowDefinition.xml" path="//Member[@MemberName='Height']/Docs/*" />
		[System.ComponentModel.TypeConverter(typeof(GridLengthTypeConverter))]
		public GridLength Height
		{
			get { return (GridLength)GetValue(HeightProperty); }
			set { SetValue(HeightProperty, value); }
		}

		/// <summary>
		/// Gets or sets the minimum allowed height of the row.
		/// </summary>
		public double MinHeight
		{
			get { return (double)GetValue(MinHeightProperty); }
			set { SetValue(MinHeightProperty, value); }
		}

		/// <summary>
		/// Gets or sets the maximum allowed height of the row.
		/// </summary>
		public double MaxHeight
		{
			get { return (double)GetValue(MaxHeightProperty); }
			set { SetValue(MaxHeightProperty, value); }
		}

		internal double ActualHeight { get; set; }

		internal double MinimumHeight { get; set; } = -1;

		public event EventHandler SizeChanged;

		void OnSizeChanged() => SizeChanged?.Invoke(this, EventArgs.Empty);
	}
}
