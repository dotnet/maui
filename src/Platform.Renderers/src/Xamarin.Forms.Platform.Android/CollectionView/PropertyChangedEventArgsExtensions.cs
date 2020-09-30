using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Xamarin.Forms.Platform.Android
{
	internal static class PropertyChangedEventArgsExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Is(this PropertyChangedEventArgs args, BindableProperty property)
		{
			return args.PropertyName == property.PropertyName;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsOneOf(this PropertyChangedEventArgs args, BindableProperty p0, BindableProperty p1)
		{
			return args.PropertyName == p0.PropertyName ||
				args.PropertyName == p1.PropertyName;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsOneOf(this PropertyChangedEventArgs args, BindableProperty p0, BindableProperty p1, BindableProperty p2)
		{
			return args.PropertyName == p0.PropertyName ||
				args.PropertyName == p1.PropertyName ||
				args.PropertyName == p2.PropertyName;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsOneOf(this PropertyChangedEventArgs args, BindableProperty p0, BindableProperty p1, BindableProperty p2, BindableProperty p3)
		{
			return args.PropertyName == p0.PropertyName ||
				args.PropertyName == p1.PropertyName ||
				args.PropertyName == p2.PropertyName ||
				args.PropertyName == p3.PropertyName;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsOneOf(this PropertyChangedEventArgs args, BindableProperty p0, BindableProperty p1, BindableProperty p2, BindableProperty p3, BindableProperty p4)
		{
			return args.PropertyName == p0.PropertyName ||
				args.PropertyName == p1.PropertyName ||
				args.PropertyName == p2.PropertyName ||
				args.PropertyName == p3.PropertyName ||
				args.PropertyName == p4.PropertyName;
		}
	}
}