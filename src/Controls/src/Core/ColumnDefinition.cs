using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ColumnDefinition.xml" path="Type[@FullName='Microsoft.Maui.Controls.ColumnDefinition']/Docs" />
	public sealed class ColumnDefinition : BindableObject, IDefinition, IGridColumnDefinition
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/ColumnDefinition.xml" path="//Member[@MemberName='WidthProperty']/Docs" />
		public static readonly BindableProperty WidthProperty = BindableProperty.Create("Width", typeof(GridLength), typeof(ColumnDefinition), new GridLength(1, GridUnitType.Star),
			propertyChanged: (bindable, oldValue, newValue) => ((ColumnDefinition)bindable).OnSizeChanged());

		/// <include file="../../docs/Microsoft.Maui.Controls/ColumnDefinition.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public ColumnDefinition()
		{
			MinimumWidth = -1;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ColumnDefinition.xml" path="//Member[@MemberName='Width']/Docs" />
		[System.ComponentModel.TypeConverter(typeof(GridLengthTypeConverter))]
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