using System;
using System.Collections.Generic;


namespace PoolMathApp.Xaml
{
	public partial class TimelineBaseView : Grid
	{
		public TimelineBaseView()
		{
			InitializeComponent();
		}

		public static readonly BindableProperty MainContentProperty
			= BindableProperty.Create(nameof(MainContent), typeof(View), typeof(TimelineBaseView), default(View),
				propertyChanged: (obj, oldValue, newValue) =>
				{
					if (obj is TimelineBaseView self)
						self.mainContent.Content = newValue as View;
				});

		public View MainContent
		{
			get => (View)GetValue(MainContentProperty);
			set => SetValue(MainContentProperty, value);
		}
	}
}
