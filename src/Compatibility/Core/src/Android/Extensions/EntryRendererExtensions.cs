using Android.Views.InputMethods;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	internal static class EntryRendererExtensions
	{
		[PortHandler]
		internal static ImeAction ToAndroidImeAction(this ReturnType returnType)
		{
			switch (returnType)
			{
				case ReturnType.Go:
					return ImeAction.Go;
				case ReturnType.Next:
					return ImeAction.Next;
				case ReturnType.Send:
					return ImeAction.Send;
				case ReturnType.Search:
					return ImeAction.Search;
				case ReturnType.Done:
					return ImeAction.Done;
				case ReturnType.Default:
					return ImeAction.Done;
				default:
					throw new System.NotImplementedException($"ReturnType {returnType} not supported");
			}
		}

		[PortHandler]
		public static ImeAction ToAndroidImeOptions(this PlatformConfiguration.AndroidSpecific.ImeFlags flags)
		{
			switch (flags)
			{
				case PlatformConfiguration.AndroidSpecific.ImeFlags.Previous:
					return ImeAction.Previous;
				case PlatformConfiguration.AndroidSpecific.ImeFlags.Next:
					return ImeAction.Next;
				case PlatformConfiguration.AndroidSpecific.ImeFlags.Search:
					return ImeAction.Search;
				case PlatformConfiguration.AndroidSpecific.ImeFlags.Send:
					return ImeAction.Send;
				case PlatformConfiguration.AndroidSpecific.ImeFlags.Go:
					return ImeAction.Go;
				case PlatformConfiguration.AndroidSpecific.ImeFlags.None:
					return ImeAction.None;
				case PlatformConfiguration.AndroidSpecific.ImeFlags.ImeMaskAction:
					return ImeAction.ImeMaskAction;
				case PlatformConfiguration.AndroidSpecific.ImeFlags.NoPersonalizedLearning:
#pragma warning disable CA1416 // Introduced in API 26: https://developer.android.com/reference/android/view/inputmethod/EditorInfo#IME_FLAG_NO_PERSONALIZED_LEARNING
					return (ImeAction)ImeFlags.NoPersonalizedLearning;
#pragma warning restore CA1416
				case PlatformConfiguration.AndroidSpecific.ImeFlags.NoExtractUi:
					return (ImeAction)ImeFlags.NoExtractUi;
				case PlatformConfiguration.AndroidSpecific.ImeFlags.NoAccessoryAction:
					return (ImeAction)ImeFlags.NoAccessoryAction;
				case PlatformConfiguration.AndroidSpecific.ImeFlags.NoFullscreen:
					return (ImeAction)ImeFlags.NoFullscreen;
				case PlatformConfiguration.AndroidSpecific.ImeFlags.Default:
				case PlatformConfiguration.AndroidSpecific.ImeFlags.Done:
				default:
					return ImeAction.Done;
			}
		}
	}
}