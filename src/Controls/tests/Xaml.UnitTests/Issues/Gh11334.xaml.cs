using System;
using System.Collections.Generic;
using NUnit.Framework;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Xaml.Diagnostics;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh11334 : ContentPage
	{
		public Gh11334()
		{
			InitializeComponent();
		}
		public Gh11334(bool useCompiledXaml)
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
				_debuggerinitialstate = Microsoft.Maui.Controls.Xaml.Diagnostics.DebuggerHelper._mockDebuggerIsAttached;
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
				Assert.That(e.ChangeType, Is.EqualTo(VisualTreeChangeType.Remove));
				Assert.That(e.ChildIndex, Is.EqualTo(0));
				Assert.Pass();
			}

			[Test]
			public void ChildIndexOnRemove([Values(false, true)] bool useCompiledXaml)
			{
				var layout = new Gh11334(useCompiledXaml);
				VisualDiagnostics.VisualTreeChanged += OnVTChanged;
				layout.Remove(null, EventArgs.Empty);
				Assert.Fail();
			}
		}
	}
}