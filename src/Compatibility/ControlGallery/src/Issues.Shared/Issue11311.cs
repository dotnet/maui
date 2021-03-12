using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls.CustomAttributes;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Issue(IssueTracker.None, 11311, "[Regression] CollectionView NSRangeException", PlatformAffected.iOS)]
	public class Issue11311 : TestTabbedPage
	{
		const string Success = "Success";

		protected override void Init()
		{
			Device.SetFlags(new[] { "Markup_Experimental" });

			Children.Add(FirstPage());
			Children.Add(CollectionViewPage());
		}

		ContentPage FirstPage()
		{
			var firstPage = new ContentPage
			{
				Title = "The first page",
				Content = new Label { Text = Success }
			};

			firstPage.Appearing += (sender, args) =>
			{
				if (firstPage.Parent is TabbedPage tabbedPage
				&& tabbedPage.Children[1] is ContentPage collectionViewPage
				&& collectionViewPage.Content is RefreshView refreshView)
				{
					refreshView.IsRefreshing = true;
				}
			};

			return firstPage;
		}

		ContentPage CollectionViewPage()
		{
			BindingContext = new _11311ViewModel();

			var collectionView = new CollectionView
			{
				Footer = new BoxView { BackgroundColor = Color.Red, HeightRequest = 53 }
			};

			collectionView.SetBinding(ItemsView.ItemsSourceProperty, nameof(_11311ViewModel.ScoreCollectionList));
			var refreshView = new RefreshView
			{
				Content = collectionView
			};


			refreshView.SetBinding(RefreshView.CommandProperty, nameof(_11311ViewModel.PopulateCollectionCommand));
			refreshView.SetBinding(RefreshView.IsRefreshingProperty, nameof(_11311ViewModel.IsRefreshing));

			var page = new ContentPage
			{
				Title = "CollectionView Page",
				Content = refreshView
			};

			return page;
		}

		class _11311ViewModel : INotifyPropertyChanged
		{
			bool _isRefreshing;
			IEnumerable<int> _scoreCollectionList;

			public _11311ViewModel()
			{
				PopulateCollectionCommand = new Command(ExecuteRefreshCommand);
				_scoreCollectionList = Enumerable.Empty<int>();
			}

			public event PropertyChangedEventHandler PropertyChanged;

			public ICommand PopulateCollectionCommand { get; }

			public IEnumerable<int> ScoreCollectionList
			{
				get => _scoreCollectionList;
				set
				{
					_scoreCollectionList = value;
					OnPropertyChanged();
				}
			}

			public bool IsRefreshing
			{
				get => _isRefreshing;
				set
				{
					if (IsRefreshing != value)
					{
						_isRefreshing = value;
						OnPropertyChanged();
					}
				}
			}

			void ExecuteRefreshCommand()
			{
				ScoreCollectionList = Enumerable.Range(0, 100);
				IsRefreshing = false;
			}

			void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

#if UITEST
		[Test]
		public void CollectionViewWithFooterShouldNotCrashOnDisplay()
		{
			// If this hasn't already crashed, the test is passing
			RunningApp.WaitForElement(Success);
		}
#endif
	}
}
