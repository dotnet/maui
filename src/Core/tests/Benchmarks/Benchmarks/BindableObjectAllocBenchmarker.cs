using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Handlers.Benchmarks
{
	[MemoryDiagnoser]
	public class BindableObjectAllocBenchmarker
	{
		Label _label;
		Label _child;
		VerticalStackLayout _deepTreeLeaf;
		VerticalStackLayout _flatLayout;
		object _contextA, _contextB;
		bool _toggle;

		[GlobalSetup]
		public void Setup()
		{
			_label = new Label();
			_contextA = new object();
			_contextB = new object();

			// 10-level deep tree for DescendantAdded/Removed propagation
			var root = new VerticalStackLayout();
			var current = root;
			for (int depth = 0; depth < 10; depth++)
			{
				var child = new VerticalStackLayout();
				current.Add(child);
				current = child;
			}
			_child = new Label();
			_deepTreeLeaf = current;

			// Flat layout with 200 children for BindingContext propagation
			_flatLayout = new VerticalStackLayout();
			for (int i = 0; i < 200; i++)
				_flatLayout.Add(new Label());
		}

		// --- #34092: Cached PropertyChangedEventArgs / PropertyChangingEventArgs ---

		[Benchmark(Description = "SetValue (EventArgs)")]
		public void SetProperty_EventArgs()
		{
			_toggle = !_toggle;
			_label.Text = _toggle ? "a" : "b";
		}

		// --- #34093: Reuse ElementEventArgs in tree propagation ---

		[Benchmark(Description = "Add+Remove child (10-deep tree)")]
		public void AddRemoveChild_DeepTree()
		{
			_deepTreeLeaf.Add(_child);
			_deepTreeLeaf.Remove(_child);
		}

		// --- #34129: BindingContext propagation (WeakReference reuse + .ToArray() elimination) ---

		[Benchmark(Description = "Set BindingContext (200 children)")]
		public void SetBindingContext_FlatTree()
		{
			_toggle = !_toggle;
			_flatLayout.BindingContext = _toggle ? _contextA : _contextB;
		}

		// --- #34131: Lazy _triggerSpecificity dictionary ---

		[Benchmark(Description = "new Label()")]
		public Label CreateLabel() => new Label();

		[Benchmark(Description = "new Entry()")]
		public Entry CreateEntry() => new Entry();
	}
}
