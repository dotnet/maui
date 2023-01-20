using System;

namespace Microsoft.Maui
{
	public record ScrollToRequest(double HorizontalOffset, double VerticalOffset, bool Instant);
}