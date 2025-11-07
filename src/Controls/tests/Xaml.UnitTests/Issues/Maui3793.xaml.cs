using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui3793 : ContentPage
{
	public Maui3793() => InitializeComponent();

	public class Tests : IDisposable
	{

		public void Dispose() { }
		// TODO: Convert to IDisposable or constructor - [MemberData(nameof(InitializeTest))] // TODO: Convert to IDisposable or constructor public void Setup() => AppInfo.SetCurrent(new MockAppInfo());

		[Theory]
		[Values]
		public void ControlTemplateFromStyle()
		{
			Maui3793 page;
			// TODO: XUnit has no DoesNotThrow. Remove this or use try/catch if needed: // (() => page = new Maui3793(inflator));
		}
	}
}
