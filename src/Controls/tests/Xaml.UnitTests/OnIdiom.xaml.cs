using System.Linq;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Mono.Cecil.Cil;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class OnIdiom : ContentPage
	{
		public OnIdiom()
		{
			InitializeComponent();
		}

		public OnIdiom(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			MockDeviceInfo mockDeviceInfo;

			[SetUp]
			public void Setup()
			{
				DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());
			}

			[TearDown]
			public void TearDown()
			{
				DeviceInfo.SetCurrent(null);
			}

			[Test]
			public void Label_Text()
			{
				MockCompiler.Compile(typeof(OnIdiom), out var md, out bool hasLoggedErrors);
				Assert.That(md.Body.Instructions.Any(static i => i.OpCode == OpCodes.Newobj && i.Operand.ToString() == "System.Void Microsoft.Maui.Controls.Xaml.OnIdiomExtension`1<System.String>::.ctor()"));
			}
		}
	}
}