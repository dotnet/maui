using Android.Views.InputMethods;

namespace Xamarin.Forms.Platform.Android
{
	internal static class EntryRendererExtensions
	{
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
					return (ImeAction)ImeFlags.NoPersonalizedLearning;
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