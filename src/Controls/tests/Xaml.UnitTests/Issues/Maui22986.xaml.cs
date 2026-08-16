using System;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[Collection("Issue")]
public class Maui22986
{
	[Fact]
	public void LoadingUnattachedPageDoesNotEmitInvalidRootAddEvent()
	{
		bool diagnosticsInitiallyEnabled = RuntimeFeature.EnableMauiDiagnostics;
		VisualTreeChangeEventArgs invalidRootAddEvent = null;
		EventHandler<VisualTreeChangeEventArgs> handler = (_, args) =>
		{
			if (args.ChangeType == VisualTreeChangeType.Add &&
				(args.Parent is null || args.ChildIndex < 0))
			{
				invalidRootAddEvent = args;
			}
		};

		RuntimeFeature.EnableMauiDiagnostics = true;
		VisualDiagnostics.VisualTreeChanged += handler;

		try
		{
			var page = new Maui22986Page(XamlInflator.Runtime);
			bool pageRaisedInvalidRootAddEvent =
				invalidRootAddEvent is not null &&
				ReferenceEquals(invalidRootAddEvent.Child, page);

			Assert.False(
				pageRaisedInvalidRootAddEvent,
				"XAML loading must not emit a visual-tree add event for an unattached page.");
		}
		finally
		{
			VisualDiagnostics.VisualTreeChanged -= handler;
			RuntimeFeature.EnableMauiDiagnostics = diagnosticsInitiallyEnabled;
		}
	}
}

public partial class Maui22986Page : ContentPage
{
}
