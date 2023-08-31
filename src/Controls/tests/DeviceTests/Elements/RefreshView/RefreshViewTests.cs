using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.RefreshView)]
	public partial class RefreshViewTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<RefreshView, RefreshViewHandler>();
				});
			});
		}


		// 		[Fact(DisplayName = "IsRefreshing binding works")]
		// 		public async Task IsRefreshingBindingWorks()
		// 		{
		// 			SetupBuilder();

		// 			var vm = new RefreshPageViewModel();
		// 			var refreshView = new RefreshView() { BindingContext = vm };

		// 			refreshView.SetBinding(RefreshView.IsRefreshingProperty, nameof(vm.IsRefreshing), BindingMode.TwoWay);

		// 			await CreateHandlerAndAddToWindow<RefreshViewHandler>(refreshView, async handler =>
		// 			{
		// 				var platformControl = handler.PlatformView;
		// 				Assert.NotNull(platformControl);
		// 				bool? platformIsRefreshing = null;
		// #if WINDOWS
		// 				Deferral refreshCompletionDeferral = null;
		// 				platformControl.RefreshRequested += (s, e) =>
		// 				{
		// 					refreshCompletionDeferral = e.GetDeferral();
		// 					platformIsRefreshing = true;
		// 				};
		// #endif
		// 				vm.IsRefreshing = true;
		// #if ANDROID
		// 				platformIsRefreshing = platformControl.Refreshing;
		// #elif IOS
		// 				platformIsRefreshing = platformControl.IsRefreshing;
		// #elif WINDOWS

		// #endif
		// 				Assert.NotNull(platformIsRefreshing);
		// 				Assert.Equal(vm.IsRefreshing, platformIsRefreshing);
		// 				await Task.Delay(300);
		// 				vm.IsRefreshing = false;
		// #if ANDROID
		// 				platformIsRefreshing = platformControl.Refreshing;
		// #elif IOS
		// 				platformIsRefreshing = platformControl.IsRefreshing;
		// #elif WINDOWS
		// 				if(refreshCompletionDeferral != null)
		// 				{
		// 					refreshCompletionDeferral.Complete();
		// 					refreshCompletionDeferral = null;
		// 					platformIsRefreshing = false;
		// 				}
		// #endif
		// 				Assert.Equal(vm.IsRefreshing, platformIsRefreshing);
		// 				await Task.Delay(1000);
		// 			});
		// 		}

		// 		[Fact(DisplayName = "IsRefreshing binding works when started on platform")]
		// 		public async Task IsRefreshingBindingWorksFromPlatform()
		// 		{
		// 			SetupBuilder();

		// 			var vm = new RefreshPageViewModel();
		// 			var refreshView = new RefreshView() { BindingContext = vm };

		// 			refreshView.SetBinding(RefreshView.IsRefreshingProperty, nameof(vm.IsRefreshing), BindingMode.TwoWay);

		// 			await CreateHandlerAndAddToWindow<RefreshViewHandler>(refreshView, async handler =>
		// 			{
		// 				var platformControl = handler.PlatformView;
		// 				Assert.NotNull(platformControl);
		// 				bool? platformIsRefreshing = null;
		// #if WINDOWS
		// 				Deferral refreshCompletionDeferral = null;
		// 				platformControl.RefreshRequested += (s, e) =>
		// 				{
		// 					refreshCompletionDeferral = e.GetDeferral();
		// 					platformIsRefreshing = true;
		// 				};
		// 				platformControl.RequestRefresh();
		// #endif

		// #if ANDROID
		// 				platformIsRefreshing = platformControl.Refreshing = true;
		// #elif IOS
		// 				platformIsRefreshing = platformControl.IsRefreshing = true;
		// #elif WINDOWS

		// #endif
		// 				Assert.NotNull(platformIsRefreshing);
		// 				Assert.Equal(platformIsRefreshing, vm.IsRefreshing);
		// 				await Task.Delay(300);
		// 				vm.IsRefreshing = false;
		// #if ANDROID
		// 				platformIsRefreshing = platformControl.Refreshing;
		// #elif IOS
		// 				platformIsRefreshing = platformControl.IsRefreshing;
		// #elif WINDOWS
		// 				if (refreshCompletionDeferral != null)
		// 				{
		// 					refreshCompletionDeferral.Complete();
		// 					refreshCompletionDeferral = null;
		// 					platformIsRefreshing = false;
		// 				}
		// #endif
		// 				Assert.Equal(vm.IsRefreshing, platformIsRefreshing);
		// 				await Task.Delay(1000);
		// 			});
		// 		}

		// 		class RefreshPageViewModel : BaseViewModel
		// 		{
		// 			public RefreshPageViewModel()
		// 			{
		// 				Data = new ObservableCollection<string>()
		// 				{
		// 					"Item 1",
		// 					"Item 2",
		// 					"Item 3"
		// 				};
		// 			}
		// 			bool _isRefreshing;
		// 			ObservableCollection<string> _data;

		// 			public bool IsRefreshing
		// 			{
		// 				get => _isRefreshing;
		// 				set => SetProperty(ref _isRefreshing, value);
		// 			}

		// 			public ObservableCollection<string> Data
		// 			{
		// 				get => _data;
		// 				set => SetProperty(ref _data, value);
		// 			}
		// 		}

		// 		public abstract class BaseViewModel : INotifyPropertyChanged
		// 		{
		// 			protected bool SetProperty<T>(ref T backingStore, T value,
		// 			[CallerMemberName] string propertyName = "",
		// 			Action onChanged = null)
		// 			{
		// 				if (EqualityComparer<T>.Default.Equals(backingStore, value))
		// 					return false;

		// 				backingStore = value;
		// 				onChanged?.Invoke();
		// 				OnPropertyChanged(propertyName);
		// 				return true;
		// 			}

		// 			#region INotifyPropertyChanged

		// 			public event PropertyChangedEventHandler PropertyChanged;
		// 			protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
		// 			{
		// 				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		// 			}

		// 			#endregion
		// 		}
	}
}
