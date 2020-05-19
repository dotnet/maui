using System.Maui.Internals;
using System.Maui.Xaml;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Maui.CustomAttributes;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
using System.Maui.Core.UITests;
#endif

namespace System.Maui.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(UITestCategories.CarouselView)]
#endif
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7813, "CarouselView vertical layout could use a converter", PlatformAffected.All)]
	public partial class Issue7813 : ContentPage
	{
		public Issue7813()
		{
#if APP
			InitializeComponent();
#endif
			BindingContext = new Issue7813ViewModel();
		}
	}

	[Preserve(AllMembers = true)]
	public class Issue7813Model
	{
		public Color Color { get; set; }
		public string Name { get; set; }
	}

	[Preserve(AllMembers = true)]
	public class Issue7813ViewModel : BindableObject
	{
		ObservableCollection<Issue7813Model> _items;

		public Issue7813ViewModel()
		{
			LoadItems();
		}

		public ObservableCollection<Issue7813Model> Items
		{
			get { return _items; }
			set
			{
				_items = value;
				OnPropertyChanged();
			}
		}

		void LoadItems()
		{
			Items = new ObservableCollection<Issue7813Model>();

			var random = new Random();
			var items = new List<Issue7813Model>();

			for (int n = 0; n < 5; n++)
			{
				items.Add(new Issue7813Model
				{
					Color = Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
					Name = $"{n + 1}"
				});
			}

			_items = new ObservableCollection<Issue7813Model>(items);
			OnPropertyChanged(nameof(Items));

		}
	}
}