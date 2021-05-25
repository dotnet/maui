using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	internal class MauiDispatcher : Dispatcher
	{
		public static Dispatcher Instance { get; } = new MauiDispatcher();

		private MauiDispatcher()
		{
		}

#pragma warning disable CA1416 // Validate platform compatibility
		public override bool CheckAccess()
		{
			return !Device.IsInvokeRequired;
		}

		public override Task InvokeAsync(Action workItem)
		{
			return Device.InvokeOnMainThreadAsync(workItem);
		}

		public override Task InvokeAsync(Func<Task> workItem)
		{
			return Device.InvokeOnMainThreadAsync(workItem);
		}

		public override Task<TResult> InvokeAsync<TResult>(Func<TResult> workItem)
		{
			return Device.InvokeOnMainThreadAsync(workItem);
		}

		public override Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> workItem)
		{
			return Device.InvokeOnMainThreadAsync(workItem);
		}
#pragma warning restore CA1416 // Validate platform compatibility
	}
}
