using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <include file="../../docs/Microsoft.Maui/SizeRequest.xml" path="Type[@FullName='Microsoft.Maui.SizeRequest']/Docs/*" />
	[DebuggerDisplay("Request={Request.Width}x{Request.Height}, Minimum={Minimum.Width}x{Minimum.Height}")]
	[TypeConverter(typeof(SizeRequestTypeConverter))]
	public struct SizeRequest : IEquatable<SizeRequest>
	{
		/// <include file="../../docs/Microsoft.Maui/SizeRequest.xml" path="//Member[@MemberName='Request']/Docs/*" />
		public Size Request { get; set; }

		/// <include file="../../docs/Microsoft.Maui/SizeRequest.xml" path="//Member[@MemberName='Minimum']/Docs/*" />
		public Size Minimum { get; set; }

		/// <include file="../../docs/Microsoft.Maui/SizeRequest.xml" path="//Member[@MemberName='.ctor'][2]/Docs/*" />
		public SizeRequest(Size request, Size minimum)
		{
			Request = request;
			Minimum = minimum;
		}

		/// <include file="../../docs/Microsoft.Maui/SizeRequest.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public SizeRequest(Size request)
		{
			Request = request;
			Minimum = request;
		}

		/// <include file="../../docs/Microsoft.Maui/SizeRequest.xml" path="//Member[@MemberName='ToString']/Docs/*" />
		public override string ToString()
		{
			return string.Format("{{Request={0} Minimum={1}}}", Request, Minimum);
		}

		public bool Equals(SizeRequest other) => Request.Equals(other.Request) && Minimum.Equals(other.Minimum);

		public static implicit operator SizeRequest(Size size) => new SizeRequest(size);

		public static implicit operator Size(SizeRequest size) => size.Request;

		public override bool Equals(object? obj) => obj is SizeRequest other && Equals(other);

		public override int GetHashCode() => Request.GetHashCode() ^ Minimum.GetHashCode();

		public static bool operator ==(SizeRequest left, SizeRequest right) => left.Equals(right);

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