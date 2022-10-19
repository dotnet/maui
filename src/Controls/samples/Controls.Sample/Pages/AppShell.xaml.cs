using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System;
using Maui.Controls.Sample.Pages.ShellGalleries;
using System.Linq;

namespace Maui.Controls.Sample.Pages
{
	public partial class AppShell
	{
		public AppShell()
		{
			InitializeComponent();
			SetTabBarBackgroundColor(this, Color.FromRgba(3, 169, 244, 255));
		}
	}

	public class PageSearchHandler : SearchHandler
	{
		public IList<Data> Pages { get; set; }
		public Type SelectedItemNavigationTarget { get; set; }

		public PageSearchHandler()
		{
			Pages = new List<Data>()
			{
				new Data(typeof(ShellChromeGallery).Name, typeof(ShellChromeGallery), "dotnet_bot.png"),
				new Data(typeof(ButtonPage).Name, typeof(ButtonPage), "books.png"),
				new Data(typeof(SemanticsPage).Name, typeof(SemanticsPage), "oasis.jpg"),
			};
		}

		protected override void OnQueryChanged(string oldValue, string newValue)
		{
			base.OnQueryChanged(oldValue, newValue);

			if (string.IsNullOrWhiteSpace(newValue))
			{
				ItemsSource = Pages;
			}
			else
			{
				ItemsSource = Pages
					.Where(page => page.Name.ToLower().Contains(newValue.ToLower(), StringComparison.OrdinalIgnoreCase))
					.ToList();
			}
		}

		protected override async void OnItemSelected(object item)
		{
			base.OnItemSelected(item);

			var thing = (Data)item;

			var result = Shell.Current.Handler.MauiContext.Services.GetService(thing.Type) ??
				Activator.CreateInstance(thing.Type);

			await Shell.Current.Navigation.PushAsync(result as Page);
		}

		public class Data
		{
			public Data(string name, Type type, string imageUrl)
			{
				Name = name;
				ImageUrl = imageUrl;
				Type = type;
			}

			public string Name { get; set; }
			public string ImageUrl { get; set; }
			public Type Type { get; set; }
		}
	}
}
