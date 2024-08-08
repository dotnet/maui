using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Maui.Controls.Sample.CollectionViewGalleries;

namespace Maui.Controls.Sample.Issues
{
	internal abstract class LogListener
	{
		public abstract void Warning(string category, string message);
	}

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

	internal class BindingErrorsViewModel
	{
		readonly string[] _imageOptions = 
		{
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

	internal static class Log
	{
		static Log()
		{
			Listeners = new List<LogListener>();
		}

		public static IList<LogListener> Listeners { get; }

		public static void Warning(string category, string message)
		{
			foreach (LogListener listener in Listeners)
				listener.Warning(category, message);
		}

		public static void Warning(string category, string format, params object[] args)
		{
			Warning(category, string.Format(format, args));
		}
	}

	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.None, 68000, "Binding errors when CollectionView ItemsSource is set with a binding", PlatformAffected.Android)]
	public partial class CollectionViewBindingErrors : ContentPage
	{
		public CollectionViewBindingErrors()
		{
			InitializeComponent();

			var listener = new CountBindingErrors(BindingErrorCount);
			Log.Listeners.Add(listener);
			Disappearing += (obj, args) => { Log.Listeners.Remove(listener); };

			BindingContext = new BindingErrorsViewModel();
		}
	}
}