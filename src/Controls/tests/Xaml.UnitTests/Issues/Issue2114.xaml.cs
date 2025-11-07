using System;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Issue2114 : Application
{
	public Issue2114() => InitializeComponent();


	public class Tests
	{
		// TODO: Convert to IDisposable or constructor - [MemberData(nameof(InitializeTest))] // TODO: Convert to IDisposable or constructor public void SetUp() => Application.Current = null;

		[Theory]
		[Values]
		public void StaticResourceOnApplication(XamlInflator inflator)
		{
			var app = new Issue2114(inflator);

			Assert.True(Current.Resources.ContainsKey("ButtonStyle"));
			Assert.True(Current.Resources.ContainsKey("NavButtonBlueStyle"));
			Assert.True(Current.Resources.ContainsKey("NavButtonGrayStyle"));
		}
	}
}