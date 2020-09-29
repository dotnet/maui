using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

#if UITEST
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.CarouselView)]
#endif
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8638, "[Bug] CarouselView inconsistent layout positioning", PlatformAffected.Android)]
	public partial class Issue8638 : TestContentPage
	{
		public Issue8638()
		{
#if APP
			Title = "Issue 8638";
			InitializeComponent();
			BindingContext = new Issue8638ViewModel();
#endif
		}

		protected override void Init()
		{

		}

		void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
		{
#if APP
			var peekAreaInsets = e.NewValue;
			CarouselView.PeekAreaInsets = new Thickness(peekAreaInsets, 0);
#endif
		}
	}

	[Preserve(AllMembers = true)]
	public class Issue8638Model
	{
		public Color Color { get; set; }
		public string Name { get; set; }
	}

	[Preserve(AllMembers = true)]
	public class Issue8638ViewModel : BindableObject
	{
		ObservableCollection<Issue8638Model> _items;

		public Issue8638ViewModel()
		{
			LoadItems();
		}

		public ObservableCollection<Issue8638Model> Items
		{
			get { return _items; }
			set
			{
				_items = value;
				OnPropertyChanged();
			}
		}

		public void LoadItems()
		{
			Items = new ObservableCollection<Issue8638Model>();

			var random = new Random();
			var items = new List<Issue8638Model>();

			for (int n = 0; n < 5; n++)
			{
				items.Add(new Issue8638Model
				{
					Color = Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
					Name = $"{n + 1}"
				});
			}

			_items = new ObservableCollection<Issue8638Model>(items);
			OnPropertyChanged(nameof(Items));
		}
	}
}