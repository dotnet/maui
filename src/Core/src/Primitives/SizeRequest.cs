using System.Diagnostics;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <include file="../../docs/Microsoft.Maui/SizeRequest.xml" path="Type[@FullName='Microsoft.Maui.SizeRequest']/Docs" />
	[DebuggerDisplay("Request={Request.Width}x{Request.Height}, Minimum={Minimum.Width}x{Minimum.Height}")]
	public struct SizeRequest
	{
		/// <include file="../../docs/Microsoft.Maui/SizeRequest.xml" path="//Member[@MemberName='Request']/Docs" />
		public Size Request { get; set; }

		/// <include file="../../docs/Microsoft.Maui/SizeRequest.xml" path="//Member[@MemberName='Minimum']/Docs" />
		public Size Minimum { get; set; }

		/// <include file="../../docs/Microsoft.Maui/SizeRequest.xml" path="//Member[@MemberName='.ctor'][2]/Docs" />
		public SizeRequest(Size request, Size minimum)
		{
			Request = request;
			Minimum = minimum;
		}

		/// <include file="../../docs/Microsoft.Maui/SizeRequest.xml" path="//Member[@MemberName='.ctor'][1]/Docs" />
		public SizeRequest(Size request)
		{
			Request = request;
			Minimum = request;
		}

		/// <include file="../../docs/Microsoft.Maui/SizeRequest.xml" path="//Member[@MemberName='ToString']/Docs" />
		public override string ToString()
		{
			return string.Format("{{Request={0} Minimum={1}}}", Request, Minimum);
		}

		public static implicit operator SizeRequest(Size size) => new SizeRequest(size);

		public static implicit operator Size(SizeRequest size) => size.Request;
	}
}