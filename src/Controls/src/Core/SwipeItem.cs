using System;
using System.ComponentModel;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/SwipeItem.xml" path="Type[@FullName='Microsoft.Maui.Controls.SwipeItem']/Docs" />
	public partial class SwipeItem : MenuItem, Controls.ISwipeItem
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeItem.xml" path="//Member[@MemberName='BackgroundColorProperty']/Docs" />
		public static readonly BindableProperty BackgroundColorProperty = BindableProperty.Create(nameof(BackgroundColor), typeof(Color), typeof(SwipeItem), null);

		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeItem.xml" path="//Member[@MemberName='IsVisibleProperty']/Docs" />
		public static readonly BindableProperty IsVisibleProperty = BindableProperty.Create(nameof(IsVisible), typeof(bool), typeof(SwipeItem), true);

		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeItem.xml" path="//Member[@MemberName='BackgroundColor']/Docs" />
		public Color BackgroundColor
		{
			get { return (Color)GetValue(BackgroundColorProperty); }
			set { SetValue(BackgroundColorProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeItem.xml" path="//Member[@MemberName='IsVisible']/Docs" />
		public bool IsVisible
		{
			get { return (bool)GetValue(IsVisibleProperty); }
			set { SetValue(IsVisibleProperty, value); }
		}

		public event EventHandler<EventArgs> Invoked;
	}
}