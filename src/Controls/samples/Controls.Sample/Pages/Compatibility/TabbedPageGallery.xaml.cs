using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using AndroidSpecific = Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;

namespace Maui.Controls.Sample.Pages
{
	public partial class TabbedPageGallery
	{
		public TabbedPageGallery()
		{
			InitializeComponent();
			this.Children.Add(new NavigationGallery());
			this.Children.Add(new NavigationPage(new NavigationGallery()) { Title = "With Nav Page" });
		}
	}
}