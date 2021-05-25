using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Controls.Platform
{
	internal interface ITitleViewRendererController
	{
		View TitleView { get; }
		FrameworkElement TitleViewPresenter { get; }
		Visibility TitleViewVisibility { get; set; }
		CommandBar CommandBar { get; }
	}
}
