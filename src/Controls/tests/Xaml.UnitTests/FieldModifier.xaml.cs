using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class FieldModifier : ContentPage
{
	public FieldModifier() => InitializeComponent();

	[TestFixture]
	class FindByNameTests
	{
		[Test]
		public void TestFieldModifier([Values] XamlInflator inflator)
		{
			var layout = new FieldModifier();
			Assert.That(layout.privateLabel, Is.Not.Null);
			Assert.That(layout.internalLabel, Is.Not.Null);
			Assert.That(layout.publicLabel, Is.Not.Null);

			var fields = typeof(FieldModifier).GetTypeInfo().DeclaredFields;

			Assert.That(fields.First(fi => fi.Name == "privateLabel").IsPrivate, Is.True);

			Assert.That(fields.First(fi => fi.Name == "internalLabel").IsPrivate, Is.False);
			Assert.That(fields.First(fi => fi.Name == "internalLabel").IsPublic, Is.False);

			Assert.That(fields.First(fi => fi.Name == "publicLabel").IsPublic, Is.True);
		}
	}
}
