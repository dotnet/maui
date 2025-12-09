using Android.Content;

namespace Microsoft.Maui.Platform
{
	/// <summary>
	/// Helper class for Material Design version detection and configuration on Android.
	/// </summary>
	/// <remarks>
	/// This helper provides runtime detection of whether Material Design 3 is enabled
	/// through the UseMaterial3 MSBuild property. The configuration is cached after the
	/// first access for performance.
	/// </remarks>
	internal static class MaterialDesignHelper
	{
		private static bool? _isMaterial3Enabled;

		/// <summary>
		/// Determines whether Material Design 3 is enabled for the application.
		/// </summary>
		/// <param name="context">The Android context (optional, reserved for future use).</param>
		/// <returns>
		/// <c>true</c> if Material Design 3 is enabled (UseMaterial3=true); 
		/// <c>false</c> if Material Design 2 is active (default).
		/// </returns>
		/// <remarks>
		/// <para>
		/// This method reads the Material Design version configuration from the runtime
		/// feature switch set by the UseMaterial3 MSBuild property. The value is cached
		/// after the first call for performance.
		/// </para>
		/// <para>
		/// Material Design 2 (AppCompat) is the default to maintain backward compatibility.
		/// Material Design 3 must be explicitly enabled via the UseMaterial3 property.
		/// </para>
		/// </remarks>
		public static bool IsMaterial3Enabled(Context? context = null)
		{
			// Return cached value if available
			if (_isMaterial3Enabled.HasValue)
				return _isMaterial3Enabled.Value;

			// Read from RuntimeFeature (configured via MSBuild)
			_isMaterial3Enabled = RuntimeFeature.IsMaterial3Enabled;

			return _isMaterial3Enabled.Value;
		}

		/// <summary>
		/// Gets the Material Design version number currently in use.
		/// </summary>
		/// <param name="context">The Android context (optional, reserved for future use).</param>
		/// <returns>2 for Material Design 2 (default), or 3 for Material Design 3.</returns>
		public static int GetMaterialDesignVersion(Context? context = null)
		{
			return IsMaterial3Enabled(context) ? 3 : 2;
		}

		/// <summary>
		/// Resets the cached Material Design version. 
		/// </summary>
		/// <remarks>
		/// This method is intended for testing purposes only and should not be called
		/// in production code. The Material Design version is determined at build time
		/// and should not change during application runtime.
		/// </remarks>
		internal static void ResetCache()
		{
			_isMaterial3Enabled = null;
		}
	}
}
