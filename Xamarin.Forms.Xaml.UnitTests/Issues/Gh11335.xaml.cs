using System;
using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;
using Xamarin.Forms.Xaml.Diagnostics;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Gh11335 : ContentPage
	{
		public Gh11335() => InitializeComponent();
		public Gh11335(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		void Remove(object sender, EventArgs e) => stack.Children.Remove(label);

		void Add(object sender, EventArgs e)
		{
			int index = stack.Children.IndexOf(label);
			Label newLabel = new Label { Text = "New Inserted Label" };
			stack.Children.Insert(index + 1, newLabel);
		}

		[TestFixture]
		class Tests
		{
			bool _debuggerinitialstate;

			[SetUp]
			public void Setup()
			{
				Device.PlatformServices = new MockPlatformServices();
				_debuggerinitialstate = Xamarin.Forms.Xaml.Diagnostics.DebuggerHelper._mockDebuggerIsAttached;
				DebuggerHelper._mockDebuggerIsAttached = true;
			}

			[TearDown]
			public void TearDown()
			{
				DebuggerHelper._mockDebuggerIsAttached = _debuggerinitialstate;
				Device.PlatformServices = null;
				VisualDiagnostics.VisualTreeChanged -= OnVTChanged;
			}

			void OnVTChanged(object sender, VisualTreeChangeEventArgs e)
			{
				Assert.That(e.ChangeType, Is.EqualTo(VisualTreeChangeType.Add));
				Assert.That(e.ChildIndex, Is.EqualTo(1));
				Assert.Pass();
			}

			[Test]
			public void ChildIndexOnAdd([Values(false, true)] bool useCompiledXaml)
			{
				var layout = new Gh11335(useCompiledXaml);
				VisualDiagnostics.VisualTreeChanged += OnVTChanged;
				layout.Add(null, EventArgs.Empty);
				Assert.Fail();
			}
		}
	}
}