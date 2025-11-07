using System.Linq;
using System.Reflection;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class FieldModifier : ContentPage
{
	public FieldModifier() => InitializeComponent();


	public class FindByNameTests
	{
		[Theory]
		[Values]
		public void TestFieldModifier()
		{
			var layout = new FieldModifier();
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
