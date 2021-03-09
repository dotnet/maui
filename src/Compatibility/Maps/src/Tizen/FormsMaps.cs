using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Tizen.Maps;
using Microsoft.Maui.Controls.Compatibility.Maps.Tizen;
using Microsoft.Maui.Controls.Platform.Tizen;

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

			FormsApplication.RequestingUserConsentFunc = new Func<Task>(() =>
			{
				TaskCompletionSource<bool> userConsentWaiter = new TaskCompletionSource<bool>();
				UserConsentAction(userConsentWaiter);
				return userConsentWaiter.Task;
			});
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