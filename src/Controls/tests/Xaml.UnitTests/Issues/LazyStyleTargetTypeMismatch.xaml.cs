using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class LazyStyleTargetTypeMismatch
{
	public LazyStyleTargetTypeMismatch() => InitializeComponent();

	[Collection("Issue")]
	public class Tests : IDisposable
	{
		public Tests()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose() => AppInfo.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void StyleIsOnlyAppliedToMatchingTargetType(XamlInflator inflator)
		{
			var style = (Style)new LazyStyleTargetTypeMismatch(inflator)["EntryStyle"];

			var editorBeforeInitialization = new Editor { Style = style };
			var entry = new Entry { Style = style };
			var editorAfterInitialization = new Editor { Style = style };

			Assert.Equal(Colors.Red, entry.TextColor);
			if (inflator == XamlInflator.SourceGen)
			{
				Assert.Null(editorBeforeInitialization.TextColor);
				Assert.Null(editorAfterInitialization.TextColor);
			}
			else
			{
				// Eagerly created styles keep applying compatible setters to mismatched elements.
				Assert.Equal(Colors.Red, editorBeforeInitialization.TextColor);
				Assert.Equal(Colors.Red, editorAfterInitialization.TextColor);
			}
		}
	}
}
