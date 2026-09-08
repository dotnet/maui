using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Compatibility.Maps.Tizen;
using Microsoft.Maui.Controls.Compatibility.Platform.Tizen;
using Tizen.Maps;

namespace Microsoft.Maui.Controls
{
	public static class FormsMaps
	{
		static MapService _mapService = null;

		internal static bool IsInitialized { get; private set; }

		internal static MapService MapService
		{
			get
			{
				Debug.Assert(_mapService != null, "FormsMaps is not initialized properly.");
				return _mapService;
			}
		}

		public static void Init(string provider, string authenticationToken)
		{
			if (IsInitialized)
				return;

			_mapService = new MapService(provider, authenticationToken);
#pragma warning disable CS0612 // Type or member is obsolete
			FormsApplication.RequestingUserConsentFunc = new Func<Task>(() =>
			{
				TaskCompletionSource<bool> userConsentWaiter = new TaskCompletionSource<bool>();
				UserConsentAction(userConsentWaiter);
				return userConsentWaiter.Task;
			});
#pragma warning disable CS0612 // Type or member is obsolete
		}

		static async void UserConsentAction(TaskCompletionSource<bool> tcs)
		{
			var result = await MapService.RequestUserConsent();
			if (result)
			{
				GeocoderBackend.Register();
				IsInitialized = true;
			}
			else
			{
				Log.Warn("Failed to get user consent which is required to use map service.");
			}
			tcs.SetResult(result);
		}
	}
}