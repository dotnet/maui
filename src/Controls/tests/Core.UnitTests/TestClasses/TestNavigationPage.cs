using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class TestNavigationPage : NavigationPage
	{
		internal TestNavigationPage(bool setforMaui, Page root = null, bool setHandler = true) : base(setforMaui, root)
		{
			Title = "Title";
			if (setforMaui && setHandler)
			{
				base.Handler = new TestNavigationHandler();
			}
		}

		public new TestNavigationHandler Handler =>
			base.Handler as TestNavigationHandler;

		public void ValidateNavigationCompleted()
		{
			Assert.Null(CurrentNavigationTask);
			if (Handler is TestNavigationHandler nh)
				Assert.Null(nh.CurrentNavigationRequest);
		}

		public async Task<bool> SendBackButtonPressedAsync()
		{
			var result = base.SendBackButtonPressed();
			var task = base.CurrentNavigationTask;
			if (task != null)
				await task;

			return result;
		}

		public Task NavigatingTask => Handler?.NavigatingTask ?? Task.CompletedTask;
	}

	public class TestNavigationHandler : ViewHandler<NavigationPage, object>
	{
		public static CommandMapper<IStackNavigationView, TestNavigationHandler> NavigationViewCommandMapper = new(ViewCommandMapper)
		{
			[nameof(IStackNavigation.RequestNavigation)] = RequestNavigation
		};

		public static PropertyMapper<IStackNavigationView, TestNavigationHandler> NavigationViewMapper
			   = new PropertyMapper<IStackNavigationView, TestNavigationHandler>();

		public NavigationRequest CurrentNavigationRequest { get; private set; }

		TaskCompletionSource _navigationSource;

		public Task NavigatingTask => _navigationSource?.Task ?? Task.CompletedTask;

		public async void CompleteCurrentNavigation()
		{
			if (CurrentNavigationRequest == null)
				throw new InvalidOperationException("No Active Navigation in the works");

			var newStack = CurrentNavigationRequest.NavigationStack.ToList();
			CurrentNavigationRequest = null;

			var source = _navigationSource;
			_navigationSource = null;

			if ((this as IElementHandler).VirtualView is IStackNavigation sn)
				sn.NavigationFinished(newStack);


			await Task.Delay(1);
			source.SetResult();
		}

		async void RequestNavigation(NavigationRequest navigationRequest)
		{
			if (CurrentNavigationRequest != null || _navigationSource != null)
				throw new InvalidOperationException("Already Processing Navigation");


			_navigationSource = new TaskCompletionSource();
			CurrentNavigationRequest = navigationRequest;

			await Task.Delay(10);
			CompleteCurrentNavigation();
		}

		public static void RequestNavigation(TestNavigationHandler arg1, IStackNavigationView arg2, object arg3)
		{
			arg1.RequestNavigation((NavigationRequest)arg3);
		}

		public TestNavigationHandler() : base(NavigationViewMapper, NavigationViewCommandMapper)
		{
		}

		protected override object CreatePlatformView()
		{
			return new object();
		}
	}
}
