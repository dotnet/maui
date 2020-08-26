using System;
using System.Collections.Generic;
using System.Text;
using Android.OS;

namespace Xamarin.Forms
{
	// TODO I know EZ simplified these
	public static partial class Forms
	{
		static BuildVersionCodes? s_sdkInt;
		static bool? s_isLollipopOrNewer;
		static bool? s_is29OrNewer;
		static bool? s_isMarshmallowOrNewer;
		static bool? s_isNougatOrNewer;
		static bool? s_isOreoOrNewer;
		static bool? s_isJellyBeanMr1OrNewer;

		internal static BuildVersionCodes SdkInt
		{
			get
			{
				if (!s_sdkInt.HasValue)
					s_sdkInt = Build.VERSION.SdkInt;
				return (BuildVersionCodes)s_sdkInt;
			}
		}

		internal static bool Is29OrNewer
		{
			get
			{
				if (!s_is29OrNewer.HasValue)
					s_is29OrNewer = (int)SdkInt >= 29;
				return s_is29OrNewer.Value;
			}
		}

		internal static bool IsJellyBeanMr1OrNewer
		{
			get
			{
				if (!s_isJellyBeanMr1OrNewer.HasValue)
					s_isJellyBeanMr1OrNewer = SdkInt >= BuildVersionCodes.JellyBeanMr1;
				return s_isJellyBeanMr1OrNewer.Value;
			}
		}

		internal static bool IsLollipopOrNewer
		{
			get
			{
				if (!s_isLollipopOrNewer.HasValue)
					s_isLollipopOrNewer = SdkInt >= BuildVersionCodes.Lollipop;
				return s_isLollipopOrNewer.Value;
			}
		}

		internal static bool IsMarshmallowOrNewer
		{
			get
			{
				if (!s_isMarshmallowOrNewer.HasValue)
					s_isMarshmallowOrNewer = SdkInt >= BuildVersionCodes.M;
				return s_isMarshmallowOrNewer.Value;
			}
		}

		internal static bool IsNougatOrNewer
		{
			get
			{
				if (!s_isNougatOrNewer.HasValue)
					s_isNougatOrNewer = SdkInt >= BuildVersionCodes.N;
				return s_isNougatOrNewer.Value;
			}
		}

		internal static bool IsOreoOrNewer
		{
			get
			{
				if (!s_isOreoOrNewer.HasValue)
					s_isOreoOrNewer = SdkInt >= BuildVersionCodes.O;
				return s_isOreoOrNewer.Value;
			}
		}
	}
}
