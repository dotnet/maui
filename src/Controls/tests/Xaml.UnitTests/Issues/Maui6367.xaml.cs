using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui6367
{
	public Maui6367() => InitializeComponent();

	[Collection("Issue")]
	public class Test : IDisposable
	{
		public Test() => AppInfo.SetCurrent(new MockAppInfo());
		public void Dispose() => AppInfo.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void NestedTriggers(XamlInflator inflator)
		{
			Maui6367 view = new Maui6367(inflator);
		}
	}
}

