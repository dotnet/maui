using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Xamarin.Forms
{
	public interface IBorderController : INotifyPropertyChanged
	{
		BindableProperty CornerRadiusProperty { get; }
		BindableProperty BorderColorProperty { get; }
		BindableProperty BorderWidthProperty { get; }
		int CornerRadius { get; }
		Color BorderColor { get; }
		Color BackgroundColor { get; }
		double BorderWidth { get; }
		bool IsSet(BindableProperty targetProperty);
	}
}
