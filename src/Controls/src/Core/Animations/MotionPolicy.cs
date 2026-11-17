#nullable enable

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>
	/// Reads the OS reduced-motion setting (spec, Reduced motion).
	/// </summary>
	internal static partial class MotionPolicy
	{
		/// <summary>When set, overrides the OS setting. For tests and demos.</summary>
		internal static bool? ReducedMotionOverride { get; set; }

		public static bool IsReducedMotion => ReducedMotionOverride ?? PlatformIsReducedMotion();

		/// <summary>
		/// The factor the OS applies to animation durations. Android's developer setting scales native
		/// animators; the new engine follows it. Scale 0 is reported as reduced motion instead.
		/// </summary>
		public static double DurationScale
		{
			get
			{
				if (ReducedMotionOverride.HasValue)
					return 1.0;

				var scale = PlatformDurationScale();
				return scale > 0 ? scale : 1.0;
			}
		}

		private static partial bool PlatformIsReducedMotion();

		private static partial double PlatformDurationScale();
	}
}
