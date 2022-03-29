using System;
using System.Threading.Tasks;
using Android.Content.PM;
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.OS;

using Camera = Android.Hardware.Camera;

namespace Microsoft.Maui.Essentials.Implementations
{
	public class FlashlightImplementation : IFlashlight
	{
		static readonly object locker = new object();

#pragma warning disable CS0618
		Camera camera;
#pragma warning restore CS0618
		SurfaceTexture surface;

		internal bool IsSupported
			=> Platform.HasSystemFeature(PackageManager.FeatureCameraFlash);

		internal bool AlwaysUseCameraApi { get; set; } = false;

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
						var cameraManager = Platform.CameraManager;
						foreach (var id in cameraManager.GetCameraIdList())
						{
							var hasFlash = cameraManager.GetCameraCharacteristics(id).Get(CameraCharacteristics.FlashInfoAvailable);
							if (Java.Lang.Boolean.True.Equals(hasFlash))
							{
								try
								{
									cameraManager.SetTorchMode(id, switchOn);
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
#pragma warning restore CS0618 // Type or member is obsolete
							camera = null;
							surface.Dispose();
							surface = null;
						}
					}
				}
			});
		}
	}
}
