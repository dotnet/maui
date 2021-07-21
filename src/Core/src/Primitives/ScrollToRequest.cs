using System;

namespace Microsoft.Maui
{
	public record ScrollToRequest(double HoriztonalOffset, double VerticalOffset, bool Instant);
}