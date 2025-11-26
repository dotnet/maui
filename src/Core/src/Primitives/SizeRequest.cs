using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// Struct that defines a preferred and minimum <see cref="Size"/> for layout measurement.
	/// </summary>
	[DebuggerDisplay("Request={Request.Width}x{Request.Height}, Minimum={Minimum.Width}x{Minimum.Height}")]
	[TypeConverter(typeof(SizeRequestTypeConverter))]
	[Obsolete("Use Microsoft.Maui.Size instead.")]
	public struct SizeRequest : IEquatable<SizeRequest>
	{
		/// <summary>
		/// Gets or sets the size that is preferred.
		/// </summary>
		public Size Request { get; set; }

		/// <summary>
		/// Gets or sets the minimum acceptable size.
		/// </summary>
		public Size Minimum { get; set; }

		/// <summary>
		/// Creates a new <see cref="SizeRequest"/> with the specified preferred and minimum sizes.
		/// </summary>
		/// <param name="request">The preferred size.</param>
		/// <param name="minimum">The minimum acceptable size.</param>
		public SizeRequest(Size request, Size minimum)
		{
			Request = request;
			Minimum = minimum;
		}

		/// <summary>
		/// Creates a new <see cref="SizeRequest"/> where preferred and minimum sizes are the same.
		/// </summary>
		/// <param name="request">The size to use for both Request and Minimum.</param>
		public SizeRequest(Size request)
		{
			Request = request;
			Minimum = request;
		}

		/// <summary>
		/// Returns a string representation of this <see cref="SizeRequest"/>.
		/// </summary>
		/// <returns>A string in the format "{Request} Minimum={Minimum}".</returns>
		public override string ToString()
		{
			return string.Format("{Request={0} Minimum={1}}", Request, Minimum);
		}

		/// <summary>
		/// Determines whether this SizeRequest is equal to another.
		/// </summary>
		/// <param name="other">The other SizeRequest to compare.</param>
		/// <returns><see langword="true"/> if both Request and Minimum are equal; otherwise <see langword="false"/>.</returns>
		public bool Equals(SizeRequest other) => Request.Equals(other.Request) && Minimum.Equals(other.Minimum);

		/// <summary>
		/// Implicitly converts a <see cref="Size"/> to a <see cref="SizeRequest"/>, using the size for both Request and Minimum.
		/// </summary>
		/// <param name="size">The size to convert.</param>
		public static implicit operator SizeRequest(Size size) => new SizeRequest(size);

		/// <summary>
		/// Implicitly converts this <see cref="SizeRequest"/> to its <see cref="Request"/> <see cref="Size"/>.
		/// </summary>
		/// <param name="size">The SizeRequest to convert.</param>
		public static implicit operator Size(SizeRequest size) => size.Request;

		/// <summary>
		/// Determines whether this instance and a specified object are equal.
		/// </summary>
		/// <param name="obj">The object to compare with this instance.</param>
		/// <returns><see langword="true"/> if <paramref name="obj"/> is a SizeRequest equal to this instance; otherwise <see langword="false"/>.</returns>
		public override bool Equals(object? obj) => obj is SizeRequest other && Equals(other);

		/// <summary>
		/// Returns a hash code for this SizeRequest.
		/// </summary>
		/// <returns>A 32-bit signed hash code.</returns>
		public override int GetHashCode() => Request.GetHashCode() ^ Minimum.GetHashCode();

		/// <summary>
		/// Indicates whether two SizeRequest instances are equal.
		/// </summary>
		/// <param name="left">The first SizeRequest to compare.</param>
		/// <param name="right">The second SizeRequest to compare.</param>
		/// <returns><see langword="true"/> if both are equal; otherwise <see langword="false"/>.</returns>
		public static bool operator ==(SizeRequest left, SizeRequest right) => left.Equals(right);

		/// <summary>
		/// Indicates whether two SizeRequest instances are not equal.
		/// </summary>
		/// <param name="left">The first SizeRequest to compare.</param>
		/// <param name="right">The second SizeRequest to compare.</param>
		/// <returns><see langword="true"/> if they differ; otherwise <see langword="false"/>.</returns>
		public static bool operator !=(SizeRequest left, SizeRequest right) => !(left == right);

		private sealed class SizeRequestTypeConverter : TypeConverter
		{
			public override bool CanConvertFrom(ITypeDescriptorContext? context, Type? sourceType)
				=> sourceType == typeof(Size);
			public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object? value)
				=> value switch
				{
					Size size => (SizeRequest)size,
					_ => throw new NotSupportedException(),
				};

			public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
				=> destinationType == typeof(Size);
			public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type? destinationType)
			{
				if (value is SizeRequest sizeRequest)
				{
					if (destinationType == typeof(Size))
						return (Size)sizeRequest;
				}

				throw new NotSupportedException();
			}
		}
	}
}