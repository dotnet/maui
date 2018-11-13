using System.ComponentModel;

namespace Xamarin.Forms.Platform.iOS
{
	internal static class PropertyChangedEventArgsExtensions
	{
		public static bool Is(this PropertyChangedEventArgs args, BindableProperty property)
		{
			return args.PropertyName == property.PropertyName;
		}

		public static bool IsOneOf(this PropertyChangedEventArgs args, params BindableProperty[] properties)
		{
			foreach (BindableProperty property in properties)
			{
				if (args.PropertyName == property.PropertyName)
				{
					return true;
				}
			}

			return false;
		}
	}
}