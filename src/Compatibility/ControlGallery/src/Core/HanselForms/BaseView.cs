//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.ControlGallery
{
	public class BaseView : ContentPage
	{
		public BaseView()
		{
			SetBinding(Page.TitleProperty, new Binding(HBaseViewModel.TitlePropertyName));
			SetBinding(Page.IconImageSourceProperty, new Binding(HBaseViewModel.IconPropertyName));
		}
	}

	public class HanselmanNavigationPage : NavigationPage
	{
		public HanselmanNavigationPage(Page root) : base(root)
		{
			Init();
		}

		public HanselmanNavigationPage()
		{
			Init();
		}

		void Init()
		{

			BarBackgroundColor = Color.FromArgb("#03A9F4");
			BarTextColor = Colors.White;
		}
	}

}