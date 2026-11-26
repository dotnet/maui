#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>
	/// Whether the system asks for reduced motion, read on the UI thread when a run starts or resumes.
	/// </summary>
	/// <remarks>Register a replacement in the app's services, for example for devices that misreport the setting.
	/// <see cref="MotionPolicy.Instant"/> settles every run immediately, for UI tests.</remarks>
	[Experimental(AnimationDiagnostics.Experimental, UrlFormat = AnimationDiagnostics.ExperimentalUrl)]
	public interface IMotionPolicy
	{
		/// <summary>Whether the user or the system asks for less motion. When true, each run's
		/// <see cref="ReducedMotionBehavior"/> applies.</summary>
		bool IsReducedMotionRequested { get; }

		/// <summary>A multiplier for animation durations, as the platform applies to its own animators
		/// (Android's animator duration scale). 1 normally; 0 is reported as reduced motion instead.</summary>
		double DurationScale { get; }

		/// <summary>Raised on the UI thread when either value changes, so runs in flight can follow.</summary>
		event EventHandler? Changed;
	}

	/// <summary>The built-in motion policies.</summary>
	[Experimental(AnimationDiagnostics.Experimental, UrlFormat = AnimationDiagnostics.ExperimentalUrl)]
	public static class MotionPolicy
	{
		/// <summary>
		/// Completes every run that plays on its own synchronously at its end state, whatever its
		/// <see cref="ReducedMotionBehavior"/>. A player driven only by seeks still shows the positions sought.
		/// Use it for UI tests that need a settled screen.
		/// </summary>
		public static IMotionPolicy Instant { get; } = new InstantMotionPolicy();

		/// <summary>The platform's own reduced-motion setting.</summary>
		public static IMotionPolicy Default => PlatformMotionPolicy.Shared;

		/// <summary>Test seam: when set, every run uses this policy.</summary>
		internal static IMotionPolicy? TestOverride { get; set; }

		internal static IMotionPolicy Resolve(BindableObject? target)
		{
			if (TestOverride is IMotionPolicy testPolicy)
				return testPolicy;

			if (target is Element element && element.FindMauiContext() is IMauiContext context
				&& context.Services.GetService<IMotionPolicy>() is IMotionPolicy fromElement)
				return fromElement;

			if (Application.Current?.FindMauiContext() is IMauiContext appContext
				&& appContext.Services.GetService<IMotionPolicy>() is IMotionPolicy fromApp)
				return fromApp;

			return Default;
		}

		internal static bool IsInstant(IMotionPolicy policy) => policy is InstantMotionPolicy;

		sealed class InstantMotionPolicy : IMotionPolicy
		{
			public bool IsReducedMotionRequested => true;

			public double DurationScale => 0;

			public event EventHandler? Changed
			{
				add { }
				remove { }
			}
		}
	}

	internal sealed partial class PlatformMotionPolicy : IMotionPolicy
	{
		static PlatformMotionPolicy? s_shared;

		public static PlatformMotionPolicy Shared => s_shared ??= new PlatformMotionPolicy();

		EventHandler? _changed;

		PlatformMotionPolicy()
		{
		}

		public bool IsReducedMotionRequested => ReadReducedMotion();

		public double DurationScale => ReadDurationScale();

		public event EventHandler? Changed
		{
			add
			{
				if (_changed is null)
					StartObserving();
				_changed += value;
			}
			remove
			{
				_changed -= value;
				if (_changed is null)
					StopObserving();
			}
		}

		// Each subscriber is isolated, so one that throws doesn't stop the others from hearing the change.
		void RaiseChanged()
		{
			var changed = _changed;
			if (changed is null)
				return;

			foreach (EventHandler handler in changed.GetInvocationList())
			{
				try
				{
					handler(this, EventArgs.Empty);
				}
				catch (Exception ex)
				{
					Application.Current?.FindMauiContext()?.CreateLogger<PlatformMotionPolicy>()?.LogError(ex, "A reduced-motion change handler threw.");
				}
			}
		}

		bool ReadReducedMotion()
		{
			var reduced = false;
			ReadReducedMotionCore(ref reduced);
			return reduced;
		}

		double ReadDurationScale()
		{
			var scale = 1.0;
			ReadDurationScaleCore(ref scale);
			return scale;
		}

		partial void ReadReducedMotionCore(ref bool reduced);

		partial void ReadDurationScaleCore(ref double scale);

		partial void StartObserving();

		partial void StopObserving();
	}
}
