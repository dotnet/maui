using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Xaml.Diagnostics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
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
		}		class Tests
		{
			bool _debuggerinitialstate;

			// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp]
			public void Setup()
			{
				_debuggerinitialstate = DebuggerHelper._mockDebuggerIsAttached;
				DebuggerHelper._mockDebuggerIsAttached = true;
			}

			// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown]
			public void TearDown()
			{
				DebuggerHelper._mockDebuggerIsAttached = _debuggerinitialstate;
				VisualDiagnostics.VisualTreeChanged -= OnVTChanged;
			}

			void OnVTChanged(object sender, VisualTreeChangeEventArgs e)
			{
				Assert.Equal(VisualTreeChangeType.Add, e.ChangeType);
				Assert.Equal(1, e.ChildIndex);
				Assert.Pass();
			}

			[Theory]
			public void Method(bool useCompiledXaml)
			{
				var layout = new Gh11335(useCompiledXaml);
				VisualDiagnostics.VisualTreeChanged += OnVTChanged;
				layout.Add(null, EventArgs.Empty);
				throw new Xunit.Sdk.XunitException();
			}
		}
	}
}