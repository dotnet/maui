using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class FieldModifier : ContentPage
	{
		public FieldModifier()
		{
			InitializeComponent();
		}

		public FieldModifier (bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class FindByNameTests
		{
			[TestCase (false)]
			[TestCase (true)]
			public void TestFieldModifier (bool useCompiledXaml)
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
}
