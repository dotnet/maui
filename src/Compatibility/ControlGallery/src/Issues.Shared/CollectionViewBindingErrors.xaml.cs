using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
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
		[MovedToAppium]
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
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

	internal static class Log
	{
		static Log()
		{
			Listeners = new SynchronizedList<LogListener>();
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

	internal abstract class LogListener
	{
		public abstract void Warning(string category, string message);
	}

	internal class DelegateLogListener : LogListener
	{
		readonly Action<string, string> _log;

		public DelegateLogListener(Action<string, string> log)
		{
			if (log == null)
				throw new ArgumentNullException("log");

			_log = log;
		}

		public override void Warning(string category, string message)
		{
			_log(category, message);
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