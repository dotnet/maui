using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/RowDefinition.xml" path="Type[@FullName='Microsoft.Maui.Controls.RowDefinition']/Docs" />
	public sealed class RowDefinition : BindableObject, IDefinition, IGridRowDefinition
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/RowDefinition.xml" path="//Member[@MemberName='HeightProperty']/Docs" />
		public static readonly BindableProperty HeightProperty = BindableProperty.Create("Height", typeof(GridLength), typeof(RowDefinition), new GridLength(1, GridUnitType.Star),
			propertyChanged: (bindable, oldValue, newValue) => ((RowDefinition)bindable).OnSizeChanged());

		/// <include file="../../docs/Microsoft.Maui.Controls/RowDefinition.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public RowDefinition()
		{
			MinimumHeight = -1;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/RowDefinition.xml" path="//Member[@MemberName='Height']/Docs" />
		[System.ComponentModel.TypeConverter(typeof(GridLengthTypeConverter))]
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