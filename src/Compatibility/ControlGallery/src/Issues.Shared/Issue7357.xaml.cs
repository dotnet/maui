using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;

#if UITEST
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
using System.Linq;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.CollectionView)]
#endif
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7357, "[Android] Setting ItemSpacing creates spacing for the last item in CollectionView", PlatformAffected.Android)]
	public partial class Issue7357 : TestContentPage
	{
#if APP
		SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
		bool _isUpdatingSource;

		public Issue7357()
		{
			InitializeComponent();

			(CollectionView7357.ItemsLayout as LinearItemsLayout).ItemSpacing = 5;

			BindingContext = new ViewModel7357();
		}

		async void CollectionView_RemainingItemsThresholdReached(object sender, System.EventArgs e)
		{
			if (_isUpdatingSource)
				return;

			await _semaphoreSlim.WaitAsync();

			try
			{
				_isUpdatingSource = true;

				var itemsSource = ((sender as CollectionView).ItemsSource as ObservableCollection<Model7357>);
				var count = itemsSource.Count;

				if (count == 100)
					return;

				for (var i = count; i < count + 50; i++)
				{
					var model = new Model7357 { Text = "Item " + i };

					if (i == 99)
						model.BackgroundColor = Colors.Pink;

					itemsSource.Add(model);
				}
			}
			finally
			{
				_semaphoreSlim.Release();
				_isUpdatingSource = false;
			}
		}
#endif

		protected override void Init()
		{

		}
	}

	[Preserve(AllMembers = true)]
	public class ViewModel7357
	{
		public ObservableCollection<Model7357> ItemsSource { get; set; } = new ObservableCollection<Model7357>();

		public ViewModel7357()
		{
			for (var i = 0; i < 50; i++)
			{
				var model = new Model7357 { Text = "Item " + i };

				if (i == 49)
					model.BackgroundColor = Colors.Pink;

				ItemsSource.Add(model);
			}
		}
	}

	[Preserve(AllMembers = true)]
	public class Model7357
	{
		public string Text { get; set; }

		public Color BackgroundColor { get; set; } = Colors.Beige;

		public Model7357()
		{

		}
	}
}