using System.Diagnostics;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	[DebuggerDisplay("Request={Request.Width}x{Request.Height}, Minimum={Minimum.Width}x{Minimum.Height}")]
	public struct SizeRequest
	{
		public Size Request { get; set; }

		public Size Minimum { get; set; }

		public SizeRequest(Size request, Size minimum)
		{
			Request = request;
			Minimum = minimum;
		}

		public SizeRequest(Size request)
		{
			Request = request;
			Minimum = request;
		}

		public override string ToString()
		{
			return string.Format("{{Request={0} Minimum={1}}}", Request, Minimum);
		}

		public static implicit operator SizeRequest(Size size) => new SizeRequest(size);

		public static implicit operator Size(SizeRequest size) => size.Request;
	}
}