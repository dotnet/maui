using System.Linq;
using System.Reflection;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class FieldModifier : ContentPage
	{
		public FieldModifier()
		{
			InitializeComponent();
		}

		public FieldModifier(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		public class FindByNameTests
		{
			[InlineData(false)]
			[InlineData(true)]
			public void TestFieldModifier(bool useCompiledXaml)
			{
				var layout = new FieldModifier();
				Assert.NotNull(layout.privateLabel);
				Assert.NotNull(layout.internalLabel);
				Assert.NotNull(layout.publicLabel);

				var fields = typeof(FieldModifier).GetTypeInfo().DeclaredFields;

				Assert.True(fields.First(fi => fi.Name == "privateLabel").IsPrivate, Is.True);

				Assert.True(fields.First(fi => fi.Name == "internalLabel").IsPrivate, Is.False);
				Assert.True(fields.First(fi => fi.Name == "internalLabel").IsPublic, Is.False);

				Assert.True(fields.First(fi => fi.Name == "publicLabel").IsPublic, Is.True);
			}
		}
	}
}
