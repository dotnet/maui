using System;
using System.Windows.Input;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery
{
	[Preserve(AllMembers = true)]
	internal class RefreshViewCoreGalleryPage : CoreGalleryPage<RefreshView>
	{
		protected override bool SupportsFocus
		{
			get { return false; }
		}

		protected override bool SupportsScroll
		{
			get { return false; }
		}

		protected override void InitializeElement(RefreshView element)
		{
			base.InitializeElement(element);

			BindingContext = new RefreshCoreGalleryViewModel();

			element.Content = CreateContent();
			element.SetBinding(RefreshView.CommandProperty, "RefreshCommand");
			element.SetBinding(RefreshView.IsRefreshingProperty, "IsRefreshing");
		}

		protected override void Build(StackLayout stackLayout)
		{
			base.Build(stackLayout);

			var refreshColorContainer = new ViewContainer<RefreshView>(Test.RefreshView.RefreshColor, new RefreshView
			{
				Content = CreateContent(),
				RefreshColor = Color.Red
			});

			refreshColorContainer.View.SetBinding(RefreshView.CommandProperty, "RefreshCommand");
			refreshColorContainer.View.SetBinding(RefreshView.IsRefreshingProperty, "IsRefreshing");

			Add(refreshColorContainer);
		}

		ScrollView CreateContent()
		{
			var scrollView = new ScrollView
			{
				BackgroundColor = Color.Green,
				HeightRequest = 250
			};

			var content = new Grid();

			var refreshLabel = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				TextColor = Color.White
			};

			refreshLabel.SetBinding(Label.TextProperty, "Info");
			content.Children.Add(refreshLabel);
			scrollView.Content = content;

			return scrollView;
		}
	}

	[Preserve(AllMembers = true)]
	public class RefreshCoreGalleryViewModel : BindableObject
	{
		const int RefreshDuration = 1;

		private bool _isRefresing;
		private string _info;

		public RefreshCoreGalleryViewModel()
		{
			Info = "RefreshView (Pull To Refresh)";
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

		public string Info
		{
			get { return _info; }
			set
			{
				_info = value;
				OnPropertyChanged();
			}
		}

		public ICommand RefreshCommand => new Command(ExecuteRefresh);

		private void ExecuteRefresh()
		{
			IsRefreshing = true;

			Device.StartTimer(TimeSpan.FromSeconds(RefreshDuration), () =>
			{
				IsRefreshing = false;
				Info = "Refreshed (Pull To Refresh again)";
				return false;
			});
		}
	}
}