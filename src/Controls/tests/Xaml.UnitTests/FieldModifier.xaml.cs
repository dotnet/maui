using System;
using System.Linq;
using System.Reflection;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class FieldModifier : ContentPage
{
	public FieldModifier() => InitializeComponent();


	public class FindByNameTests : IDisposable
	{
		public FindByNameTests()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
			DispatcherProvider.SetCurrent(null);
			Application.SetCurrentApplication(null);
		}
		[Theory]
		[Values]
		public void TestFieldModifier(XamlInflator inflator)
		{
			var layout = new FieldModifier(inflator);
			Assert.NotNull(layout.privateLabel);
			Assert.NotNull(layout.internalLabel);
			Assert.NotNull(layout.publicLabel);

			var fields = typeof(FieldModifier).GetTypeInfo().DeclaredFields;

			Assert.True(fields.First(fi => fi.Name == "privateLabel").IsPrivate);

			Assert.False(fields.First(fi => fi.Name == "internalLabel").IsPrivate);
			Assert.False(fields.First(fi => fi.Name == "internalLabel").IsPublic);

			Assert.True(fields.First(fi => fi.Name == "publicLabel").IsPublic);
		}
	}
}
