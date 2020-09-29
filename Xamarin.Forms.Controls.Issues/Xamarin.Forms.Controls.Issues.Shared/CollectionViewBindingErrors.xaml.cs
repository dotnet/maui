using System;
using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.CollectionView)]
#endif
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 68000, "Binding errors when CollectionView ItemsSource is set with a binding",
		PlatformAffected.Android)]
	public partial class CollectionViewBindingErrors : TestContentPage
	{
		public CollectionViewBindingErrors()
		{
#if APP
			InitializeComponent();

			var listener = new CountBindingErrors(BindingErrorCount);
			Log.Listeners.Add(listener);
			Disappearing += (obj, args) => { Log.Listeners.Remove(listener); };

			BindingContext = new BindingErrorsViewModel();
#endif
		}

		protected override void Init()
		{

		}

#if UITEST
		[Test]
		public void CollectionViewBindingErrorsShouldBeZero()
		{
			RunningApp.WaitForElement("Binding Errors: 0");
		}
#endif
	}

	[Preserve(AllMembers = true)]
	public class CollectionViewGalleryTestItem
	{
		public DateTime Date { get; set; }
		public string Caption { get; set; }
		public string Image { get; set; }
		public int Index { get; set; }

		public CollectionViewGalleryTestItem(DateTime date, string caption, string image, int index)
		{
			Date = date;
			Caption = caption;
			Image = image;
			Index = index;
		}

		public override string ToString()
		{
			return $"Item: {Index}";
		}
	}

	[Preserve(AllMembers = true)]
	internal class CountBindingErrors : LogListener
	{
		private readonly Label _errorCount;
		int _count;

		public CountBindingErrors(Label errorCount)
		{
			_errorCount = errorCount;
		}

		public override void Warning(string category, string message)
		{
			if (category == "Binding")
			{
				_count += 1;
			}

			_errorCount.Text = $"Binding Errors: {_count}";
		}
	}

	[Preserve(AllMembers = true)]
	internal class BindingErrorsViewModel
	{
		readonly string[] _imageOptions = {
			"cover1.jpg",
			"oasis.jpg",
			"photo.jpg",
			"Vegetables.jpg",
			"Fruits.jpg",
			"FlowerBuds.jpg",
			"Legumes.jpg"
		};

		List<CollectionViewGalleryTestItem> GenerateList()
		{
			var items = new List<CollectionViewGalleryTestItem>();
			var images = _imageOptions;

			for (int n = 0; n < 100; n++)
			{
				items.Add(new CollectionViewGalleryTestItem(DateTime.Now.AddDays(n),
					$"Item: {n}", images[n % images.Length], n));
			}

			return items;
		}

		List<CollectionViewGalleryTestItem> _items;

		public List<CollectionViewGalleryTestItem> ItemsList
		{
			get
			{
				if (_items == null)
				{
					_items = GenerateList();
				}

				return _items;
			}
		}
	}
}