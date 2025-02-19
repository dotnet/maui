using System;
using System.Collections.Generic;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.RefreshViewGalleries
{
	[Preserve(AllMembers = true)]
	public partial class RefreshWebViewGallery : ContentPage
	{
		public RefreshWebViewGallery()
		{
			InitializeComponent();
			BindingContext = new RefreshWebViewGalleryViewModel();
		}
	}

	[Preserve(AllMembers = true)]
	public class RefreshWebViewGalleryViewModel : BindableObject
	{
		const int RefreshDuration = 2;

		readonly Random _random;
		bool _isRefresing;
		string _url;

		public RefreshWebViewGalleryViewModel()
		{
			_random = new Random();
			LoadUrl();
		}

		public bool IsRefreshing
		{
			get { return _isRefresing; }
			set
			{
				_isRefresing = value;
				OnPropertyChanged();
			}
		}

		public string Url
		{
			get { return _url; }
			set
			{
				_url = value;
				OnPropertyChanged();
			}
		}

		public ICommand RefreshCommand => new Command(ExecuteRefresh);

		void LoadUrl()
		{
			var urls = new List<string> { "https://dotnet.microsoft.com/apps/xamarin", "https://devblogs.microsoft.com/xamarin/" };
			int index = _random.Next(urls.Count);
			Url = urls[index];
		}

		void ExecuteRefresh()
		{
			IsRefreshing = true;

			Device.StartTimer(TimeSpan.FromSeconds(RefreshDuration), () =>
			{
				LoadUrl();

				IsRefreshing = false;

				return false;
			});
		}
	}
}