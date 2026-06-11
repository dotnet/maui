using System;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class SwitchExtensions
	{
		static UIColor DefaultBackgroundColor = UIColor.FromRGBA(120, 120, 128, 40);

		public static void UpdateIsOn(this UISwitch uiSwitch, ISwitch view)
		{
			uiSwitch.SetState(view.IsOn, true);
		}

		public static void UpdateTrackColor(this UISwitch uiSwitch, ISwitch view)
		{
			if (view is null)
			{
				return;
			}

			uiSwitch.CompleteMapperColorOverrideDetection(view);

			var styleChanged = uiSwitch.UpdatePreferredStyle(view);

			if (uiSwitch.ShouldPreserveNativeDefaults(view))
			{
				uiSwitch.ClearCustomColorState();
				uiSwitch.ReapplyNativeDefaultsAfterStyleUpdate(styleChanged);
				return;
			}

			uiSwitch.ApplyTrackColor(view);

			if (styleChanged)
			{
				uiSwitch.ReapplyColorsAfterStyleUpdate(view);
			}
		}

		internal static void ApplyTrackColor(this UISwitch uiSwitch, ISwitch view)
		{
			var uIView = GetTrackSubview(uiSwitch);

			if (uIView is null)
			{
				return;
			}

			var trackColor = view.TrackColor?.ToPlatform();

			if (trackColor is not null)
			{
				(uiSwitch as MauiSwitch)?.MarkMauiTrackColorOverride();
			}

			if (view.IsOn)
			{
				if (trackColor is not null)
				{
					uiSwitch.OnTintColor = trackColor;
					uIView.BackgroundColor = trackColor;
				}
				else
				{
					uiSwitch.OnTintColor = null;
					uIView.BackgroundColor = null;
				}
			}
			else
			{
				if (trackColor is not null)
				{
					uIView.BackgroundColor = trackColor;
				}
				else
				{
					// iOS 13+ uses the UIColor.SecondarySystemFill to support Light and Dark mode
					// else, use the RGBA equivalent of UIColor.SecondarySystemFill in Light mode
					var fallbackColor = OperatingSystem.IsIOSVersionAtLeast(13) ? UIColor.SecondarySystemFill : DefaultBackgroundColor;
					uIView.BackgroundColor = fallbackColor;
				}
			}
		}

		public static void UpdateThumbColor(this UISwitch uiSwitch, ISwitch view)
		{
			if (view == null)
				return;

			uiSwitch.CompleteMapperColorOverrideDetection(view);

			var styleChanged = uiSwitch.UpdatePreferredStyle(view);

			if (uiSwitch.ShouldPreserveNativeDefaults(view))
			{
				uiSwitch.ClearCustomColorState();
				uiSwitch.ReapplyNativeDefaultsAfterStyleUpdate(styleChanged);
				return;
			}

			uiSwitch.ApplyThumbColor(view);

			if (styleChanged)
			{
				uiSwitch.ReapplyColorsAfterStyleUpdate(view);
			}
		}

		internal static void ApplyThumbColor(this UISwitch uiSwitch, ISwitch view)
		{
			uiSwitch.ThumbTintColor = view.ThumbColor?.ToPlatform();
		}

		static bool UpdatePreferredStyle(this UISwitch uiSwitch, ISwitch view)
		{
#if IOS || MACCATALYST
			if (!IsSlidingStyleRequiredForCustomColors())
			{
				return false;
			}

			var preferredStyle = uiSwitch.HasCustomColorIntent(view)
				? UISwitchStyle.Sliding
				: UISwitchStyle.Automatic;

			if (uiSwitch.PreferredStyle != preferredStyle)
			{
				uiSwitch.PreferredStyle = preferredStyle;
				uiSwitch.SetNeedsLayout();
				return true;
			}
#endif
			return false;
		}

		static void ReapplyColorsAfterStyleUpdate(this UISwitch uiSwitch, ISwitch view)
		{
#if IOS || MACCATALYST
			if (!IsSlidingStyleRequiredForCustomColors() || !uiSwitch.HasCustomColorIntent(view))
			{
				return;
			}

			if (uiSwitch is MauiSwitch mauiSwitch)
			{
				mauiSwitch.SetNeedsColorReapply();
			}
			else if (uiSwitch.IsReadyForColorReapply())
			{
				uiSwitch.ApplyTrackColor(view);
				uiSwitch.ApplyThumbColor(view);
			}
#endif
		}

		static void ReapplyNativeDefaultsAfterStyleUpdate(this UISwitch uiSwitch, bool styleChanged)
		{
#if IOS || MACCATALYST
			if (!styleChanged || !IsSlidingStyleRequiredForCustomColors())
			{
				return;
			}

			if (uiSwitch is MauiSwitch mauiSwitch)
			{
				mauiSwitch.SetNeedsNativeDefaultCleanup();
			}
			else if (uiSwitch.IsReadyForColorReapply())
			{
				uiSwitch.ClearCustomColorState();
			}
#endif
		}

		static void CompleteMapperColorOverrideDetection(this UISwitch uiSwitch, ISwitch view)
		{
			if (view.HasCustomColors())
			{
				(uiSwitch as MauiSwitch)?.CompleteMapperColorOverrideDetection();
			}
		}

		internal static bool HasCustomColors(this ISwitch view)
		{
			return view.TrackColor is not null || view.ThumbColor is not null;
		}

		internal static bool HasCustomColorIntent(this UISwitch uiSwitch, ISwitch view)
		{
			return view.HasCustomColors() || (uiSwitch as MauiSwitch)?.HasMapperColorOverride == true;
		}

		internal static bool ShouldPreserveNativeDefaults(this ISwitch view)
		{
			return IsSlidingStyleRequiredForCustomColors() && !view.HasCustomColors();
		}

		internal static bool ShouldPreserveNativeDefaults(this UISwitch uiSwitch, ISwitch view)
		{
			return IsSlidingStyleRequiredForCustomColors() && !uiSwitch.HasCustomColorIntent(view);
		}

		internal static SwitchColorState CaptureColorState(this UISwitch uiSwitch)
		{
			return new SwitchColorState(uiSwitch.OnTintColor, uiSwitch.ThumbTintColor, uiSwitch.GetTrackColor());
		}

		internal static bool HasColorStateChanged(this UISwitch uiSwitch, SwitchColorState previousState)
		{
			return !AreEqual(uiSwitch.OnTintColor, previousState.OnTintColor)
				|| !AreEqual(uiSwitch.ThumbTintColor, previousState.ThumbTintColor)
				|| !AreEqual(uiSwitch.GetTrackColor(), previousState.TrackColor);
		}

		static bool AreEqual(UIColor? first, UIColor? second)
		{
			if (ReferenceEquals(first, second))
			{
				return true;
			}

			if (first is null || second is null)
			{
				return false;
			}

			return first.Equals(second);
		}

		internal static void ClearCustomColorState(this UISwitch uiSwitch)
		{
			uiSwitch.OnTintColor = null;
			uiSwitch.ThumbTintColor = null;

			if (uiSwitch is MauiSwitch mauiSwitch && mauiSwitch.HasMauiTrackColorOverride)
			{
				uiSwitch.GetTrackSubview()?.BackgroundColor = null;
				mauiSwitch.ClearMauiTrackColorOverride();
			}
		}

		internal static bool IsReadyForColorReapply(this UISwitch uiSwitch)
		{
			var trackSubview = uiSwitch.GetTrackSubview();

			return uiSwitch.Window is not null
				&& trackSubview is not null
				&& uiSwitch.Bounds.Width > 0
				&& uiSwitch.Bounds.Height > 0
				&& trackSubview.Bounds.Width > 0
				&& trackSubview.Bounds.Height > 0;
		}

		static bool IsSlidingStyleRequiredForCustomColors()
		{
#if IOS || MACCATALYST
			return OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26);
#else
			return false;
#endif
		}

		internal static UIView? GetTrackSubview(this UISwitch uISwitch)
		{
			if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsTvOSVersionAtLeast(13))
				return uISwitch.Subviews?.FirstOrDefaultNoLinq()?.Subviews?.FirstOrDefaultNoLinq();
			else
				return uISwitch.Subviews?.FirstOrDefaultNoLinq()?.Subviews?.FirstOrDefaultNoLinq()?.Subviews?.FirstOrDefaultNoLinq();
		}

		internal static UIColor? GetTrackColor(this UISwitch uISwitch)
		{
			return uISwitch.GetTrackSubview()?.BackgroundColor;
		}
	}

	internal readonly struct SwitchColorState
	{
		public SwitchColorState(UIColor? onTintColor, UIColor? thumbTintColor, UIColor? trackColor)
		{
			OnTintColor = onTintColor;
			ThumbTintColor = thumbTintColor;
			TrackColor = trackColor;
		}

		public UIColor? OnTintColor { get; }

		public UIColor? ThumbTintColor { get; }

		public UIColor? TrackColor { get; }
	}
}
