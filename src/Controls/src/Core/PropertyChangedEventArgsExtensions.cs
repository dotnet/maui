#nullable disable
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Controls
{
	internal static class PropertyChangedEventArgsExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Is(this PropertyChangedEventArgs args, BindableProperty property)
		{
			if (string.IsNullOrEmpty(args.PropertyName))
				return true;

			return args.PropertyName == property.PropertyName;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsOneOf(this PropertyChangedEventArgs args, BindableProperty p0, BindableProperty p1)
		{
			if (string.IsNullOrEmpty(args.PropertyName))
				return true;

			return args.PropertyName == p0.PropertyName ||
				args.PropertyName == p1.PropertyName;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsOneOf(this PropertyChangedEventArgs args, BindableProperty p0, BindableProperty p1, BindableProperty p2)
		{
			if (string.IsNullOrEmpty(args.PropertyName))
				return true;

			return args.PropertyName == p0.PropertyName ||
				args.PropertyName == p1.PropertyName ||
				args.PropertyName == p2.PropertyName;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsOneOf(this PropertyChangedEventArgs args, BindableProperty p0, BindableProperty p1, BindableProperty p2, BindableProperty p3)
		{
			if (string.IsNullOrEmpty(args.PropertyName))
				return true;

			return args.PropertyName == p0.PropertyName ||
				args.PropertyName == p1.PropertyName ||
				args.PropertyName == p2.PropertyName ||
				args.PropertyName == p3.PropertyName;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsOneOf(this PropertyChangedEventArgs args, BindableProperty p0, BindableProperty p1, BindableProperty p2, BindableProperty p3, BindableProperty p4)
		{
			if (string.IsNullOrEmpty(args.PropertyName))
				return true;

			return args.PropertyName == p0.PropertyName ||
				args.PropertyName == p1.PropertyName ||
				args.PropertyName == p2.PropertyName ||
				args.PropertyName == p3.PropertyName ||
				args.PropertyName == p4.PropertyName;
		}
	}
}