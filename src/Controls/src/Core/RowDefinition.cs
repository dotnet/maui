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

		/// <include file="../../docs/Microsoft.Maui.Controls/RowDefinition.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public RowDefinition()
		{
		}

		public RowDefinition(GridLength height)
		{
			SetValue(HeightProperty, height);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/RowDefinition.xml" path="//Member[@MemberName='Height']/Docs/*" />
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
