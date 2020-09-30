using System;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NUnit.Framework;
using Xamarin.Forms.Build.Tasks;
using Xamarin.Forms.Core.UnitTests;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class DefaultCtorRouting : ContentPage
	{
		[TypeConverter(typeof(IsCompiledTypeConverter))]
		public bool IsCompiled { get; set; }

		public DefaultCtorRouting()
		{
			InitializeComponent();
		}

		[TestFixture]
		class Tests
		{
			[SetUp]
			public void Setup()
			{
				Device.PlatformServices = new MockPlatformServices();
			}

			[TearDown]
			public void TearDown()
			{
				Device.PlatformServices = null;
			}

			[Test]
			public void ShouldntBeCompiled()
			{
				var p = new DefaultCtorRouting();
				Assert.False(p.IsCompiled);
			}
		}
	}

	[ProvideCompiled("Xamarin.Forms.Core.XamlC.IsCompiledTypeConverter")]
	class IsCompiledTypeConverter : TypeConverter, ICompiledTypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			if (value != "IsCompiled?")
				throw new Exception();
			return false;
		}

		public IEnumerable<Instruction> ConvertFromString(string value, ILContext context, BaseNode node)
		{
			if (value != "IsCompiled?")
				throw new Exception();
			yield return Instruction.Create(OpCodes.Ldc_I4_1);
		}
	}
}