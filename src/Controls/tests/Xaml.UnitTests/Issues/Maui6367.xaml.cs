using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui6367
{
	public Maui6367() => InitializeComponent();


	public class Test
	{
		// TODO: Convert to IDisposable or constructor - [MemberData(nameof(InitializeTest))] // TODO: Convert to IDisposable or constructor public void Setup() => AppInfo.SetCurrent(new MockAppInfo());

		[Theory]
		[Values]
		public void NestedTriggers(XamlInflator inflator)
		{
			Maui6367 view = new Maui6367(inflator);
		}
	}
}

