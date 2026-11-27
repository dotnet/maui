using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class TrimmableStylesDisabled
{
	public TrimmableStylesDisabled() => InitializeComponent();

	[Collection("Issue")]
	public class Tests : IDisposable
	{
		public Tests()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose() => AppInfo.SetCurrent(null);

		[Fact]
		public void StylesAreCreatedEagerlyWhenTrimmableStylesAreDisabled()
		{
			var style = (Style)new TrimmableStylesDisabled()["EntryStyle"];

			Assert.Null(style.LazyInitialization);
			Assert.Equal(InputView.TextColorProperty, Assert.Single(style.Setters).Property);

			var editor = new Editor { Style = style };
			Assert.Equal(Colors.Red, editor.TextColor);
		}
	}
}
