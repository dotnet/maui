using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	partial class NavigationFramePage
	{
		//ContentPresenter Root { get; }
		public IView? View { get; set; }
		public IMauiContext? MauiContext { get; internal set; }
		public new NavigationFrame Frame => (NavigationFrame)base.Frame;

		public NavigationFramePage()
		{
			InitializeComponent();
			//Root = new ContentPresenter()
			//{
			//	HorizontalAlignment = UI.Xaml.HorizontalAlignment.Stretch,
			//	VerticalAlignment = UI.Xaml.VerticalAlignment.Stretch
			//};

			//this.Content = Root;
		}

		protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			var stack = Frame.NavigationManager.NavigationStack;

			Load(stack[stack.Count - 1], Frame.NavigationManager.MauiContext);
		}

		public void Load()
		{
			if (View != null && MauiContext != null)
			{
				Load(View, MauiContext);
			}
		}

		public void Load(IView view, IMauiContext mauiContext)
		{
			View = view;
			MauiContext = mauiContext;
			Content = View.ToNative(mauiContext);
		}
	}
}
