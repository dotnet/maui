using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;


namespace Microsoft.Maui.Controls.Xaml.UnitTests;

//[XamlCompilation(XamlCompilationOptions.Skip)]
public partial class XShared : ContentPage
{
	public XShared() => InitializeComponent();
	public XShared(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	[TestFixture]
	class Tests
	{
		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public async Task XSharedIsSupportedOnResources([Values(false, true)]bool useCompiledXaml)
		{
			if (useCompiledXaml)
				MockCompiler.Compile(typeof(XShared));
			var layout = new XShared(useCompiledXaml);
			
			Assert.That(layout.Resources.TryGetValue("shared", out var test), Is.True, "shared");
			layout.Resources.TryGetValue("shared", out var shared);
			Assert.That(test, Is.SameAs(shared)); //shared values are shared

			layout.Resources.TryGetValue("notshared", out var notshared);
			Assert.That(layout.Resources.TryGetValue("notshared", out test), Is.True, "notshared");
			Assert.That(test, Is.Not.SameAs(notshared)); //notshared values are... not

			layout.Resources.TryGetValue("shapenotshared", out var shapenotshared);
			Assert.That(layout.Resources.TryGetValue("shapenotshared", out test), Is.True, "shapenotshared");
			Assert.That(test, Is.Not.SameAs(shapenotshared)); //notshared values are... notshared
			Assert.That(((RoundRectangle)shapenotshared).CornerRadius.TopLeft, Is.EqualTo(20));


			// Assert.That(layout.sl.Resources.TryGetValue("slnotshared", out test), Is.True, "slnotshared");
			// layout.sl.Resources.TryGetValue("slnotshared", out var slnotshared);
			// Assert.That(test, Is.Not.SameAs(slnotshared));

			Assert.That(layout.l0.Resources.TryGetValue("l0notshared", out test), Is.True, "l0notshared");
			layout.l0.Resources.TryGetValue("l0notshared", out var l0notshared);
			Assert.That(test, Is.Not.SameAs(l0notshared));

			Assert.That(layout.l1.Resources.TryGetValue("l1notshared", out test), Is.True, "l1notshared");
			layout.l1.Resources.TryGetValue("l1notshared", out var l1notshared);
			Assert.That(test, Is.Not.SameAs(l1notshared));

			var wr = new WeakReference<object>(notshared);
#pragma warning disable IDE0059 // it _is_ a necessary assignment
			notshared = null;
#pragma warning restore IDE0059 // 
			GC.Collect();
			await Task.Delay(500);
			GC.Collect();
			Assert.That(wr.TryGetTarget(out _), Is.False);
		}
	}
}