using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.OS;
using Microsoft.Maui.ApplicationModel;
using Camera = Android.Hardware.Camera;

namespace Microsoft.Maui.Devices
{
	class FlashlightImplementation : IFlashlight
	{
		static CameraManager CameraManager =>
			Application.Context.GetSystemService(Context.CameraService) as CameraManager;

		static readonly object locker = new object();

#pragma warning disable CS0618
#pragma warning disable CA1416 // Validate platform compatibility
#pragma warning disable CA1422 // Validate platform compatibility
		Camera camera;
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning restore CA1422 // Validate platform compatibility
#pragma warning restore CS0618
		SurfaceTexture surface;

		internal bool IsSupported
			=> PlatformUtils.HasSystemFeature(PackageManager.FeatureCameraFlash);

		internal bool AlwaysUseCameraApi { get; set; } = false;

		/// <summary>
		/// Checks if the flashlight is available and can be turned on or off.
		/// </summary>
		/// <returns><see langword="true"/> when the flashlight is available, or <see langword="false"/> when not</returns>
		public Task<bool> IsSupportedAsync() => Task.FromResult(IsSupported);

		public async Task TurnOnAsync()
		{
			await CheckSupportAsync();

			await ToggleTorchAsync(true);
		}

		public async Task TurnOffAsync()
		{
			await CheckSupportAsync();

			await ToggleTorchAsync(false);
		}

		async Task CheckSupportAsync()
		{
			if (!IsSupported)
				throw new FeatureNotSupportedException();

			await Permissions.EnsureGrantedAsync<Permissions.Flashlight>();
		}

		Task ToggleTorchAsync(bool switchOn)
		{
			return Task.Run(() =>
			{
				lock (locker)
				{
					if (OperatingSystem.IsAndroidVersionAtLeast(23) && !AlwaysUseCameraApi)
					{
						foreach (var id in CameraManager.GetCameraIdList())
						{
							var hasFlash = CameraManager.GetCameraCharacteristics(id).Get(CameraCharacteristics.FlashInfoAvailable);
							if (Java.Lang.Boolean.True.Equals(hasFlash))
							{
								try
								{
									CameraManager.SetTorchMode(id, switchOn);
									break;
								}
								catch (Exception ex)
								{
									System.Diagnostics.Debug.WriteLine($"Unable to SetTorchMode on {id}: {ex.Message}");
								}
							}
						}
					}
					else
					{
						if (camera == null)
						{
							if (surface == null)
								surface = new SurfaceTexture(0);

#pragma warning disable CS0618 // Camera types are deprecated in Android 10+
#pragma warning disable CA1416 // Validate platform compatibility
#pragma warning disable CA1422 // Validate platform compatibility
							camera = Camera.Open();


							// Nexus 5 and some devices require a preview texture
							camera.SetPreviewTexture(surface);

						}

						var param = camera.GetParameters();

						// Deprecated in an earlier android version
						param.FlashMode = switchOn ? Camera.Parameters.FlashModeTorch : Camera.Parameters.FlashModeOff;

						camera.SetParameters(param);

						if (switchOn)
						{
							camera.StartPreview();
						}
						else
						{
							camera.StopPreview();
							camera.Release();
							camera.Dispose();
							camera = null;
							surface.Dispose();
							surface = null;
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning restore CA1422 // Validate platform compatibility
						}
					}
				}
			});
		}
	}
}
