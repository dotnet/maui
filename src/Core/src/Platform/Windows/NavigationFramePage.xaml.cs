using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	partial class NavigationFramePage
	{
		Microsoft.UI.Xaml.Controls.ContentPresenter Root { get; }
		public IView? View { get; set; }
		public IMauiContext? MauiContext { get; internal set; }
		public new NavigationFrame Frame => (NavigationFrame)base.Frame;

		public NavigationFramePage()
		{
			InitializeComponent();
			Root = new Microsoft.UI.Xaml.Controls.ContentPresenter()
			{
				HorizontalAlignment = UI.Xaml.HorizontalAlignment.Stretch,
				VerticalAlignment = UI.Xaml.VerticalAlignment.Stretch
			};

			this.Content = Root;
		}

		protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			Load(Frame.NavigationManager.NavigationStack.Last(), Frame.NavigationManager.MauiContext);
		}

		protected override void OnNavigatedFrom(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
		{
			base.OnNavigatedFrom(e);
		}

		public void Load()
		{
			var parent = this.Parent;
			if (View != null && MauiContext != null)
			{
				Load(View, MauiContext);
			}
		}

		public void Load(IView view, IMauiContext mauiContext)
		{
			View = view;
			MauiContext = mauiContext;
			Root.Content = View.ToNative(mauiContext);
		}
	}
}
