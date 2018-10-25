using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Xamarin.Forms.Platform.UWP
{
	internal interface ITitleViewRendererController
	{
		View TitleView { get; }
		FrameworkElement TitleViewPresenter { get; }
		Visibility TitleViewVisibility { get; set; }
		CommandBar CommandBar { get; }
	}
}
