using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class TabbedPageGallery
	{
		public TabbedPageGallery()
		{
			InitializeComponent();
			this.Children.Add(new NavigationGallery());
		}
	}
}