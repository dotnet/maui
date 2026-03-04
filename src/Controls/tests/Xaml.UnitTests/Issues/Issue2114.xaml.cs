using System;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Issue2114 : Application
{
	public Issue2114() => InitializeComponent();

	[Collection("Issue")]
	public class Tests : IDisposable
	{
		public Tests() => Application.Current = null;
		public void Dispose() => Application.Current = null;

		[Theory]
		[XamlInflatorData]
		internal void StaticResourceOnApplication(XamlInflator inflator)
		{
			Issue2114 app;
			var ex = Record.Exception(() => app = new Issue2114(inflator));
			Assert.Null(ex);

			Assert.True(Application.Current.Resources.ContainsKey("ButtonStyle"));
			Assert.True(Application.Current.Resources.ContainsKey("NavButtonBlueStyle"));
			Assert.True(Application.Current.Resources.ContainsKey("NavButtonGrayStyle"));
		}
	}
}