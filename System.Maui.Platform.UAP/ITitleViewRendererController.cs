using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using global::Windows.UI.Xaml;
using global::Windows.UI.Xaml.Controls;

namespace System.Maui.Platform.UWP
{
	internal interface ITitleViewRendererController
	{
		View TitleView { get; }
		FrameworkElement TitleViewPresenter { get; }
		Visibility TitleViewVisibility { get; set; }
		CommandBar CommandBar { get; }
	}
}
