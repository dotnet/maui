using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Xaml.Diagnostics;
using Xunit;

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
		}		class Tests
		{
			bool _debuggerinitialstate;

			[SetUp]
			public void Setup()
			{
				_debuggerinitialstate = DebuggerHelper._mockDebuggerIsAttached;
				DebuggerHelper._mockDebuggerIsAttached = true;
			}			public void TearDown()
			{
				DebuggerHelper._mockDebuggerIsAttached = _debuggerinitialstate;
				VisualDiagnostics.VisualTreeChanged -= OnVTChanged;
			}

			void OnVTChanged(object sender, VisualTreeChangeEventArgs e)
			{
				Assert.Equal(VisualTreeChangeType.Remove, e.ChangeType);
				Assert.Equal(0, e.ChildIndex);
				Assert.Pass();
			}

			[Fact]
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