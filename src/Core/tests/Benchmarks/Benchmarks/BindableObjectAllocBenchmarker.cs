using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Handlers.Benchmarks
{
	[MemoryDiagnoser]
	public class BindableObjectAllocBenchmarker
	{
		// --- #34092: Cached PropertyChangedEventArgs / PropertyChangingEventArgs ---

		/// <summary>
		/// Sets the same property repeatedly to measure PropertyChanged/Changing EventArgs allocations.
		/// Before: new PropertyChangedEventArgs + new PropertyChangingEventArgs per call.
		/// After: cached per BindableProperty.
		/// </summary>
		[Benchmark]
		public void SetProperty_EventArgsAlloc()
		{
			var label = new Label();
			for (int i = 0; i < 1_000; i++)
			{
				label.Text = "a";
				label.Text = "b";
			}
		}

		/// <summary>
		/// Sets multiple different properties to show caching benefit across properties.
		/// </summary>
		[Benchmark]
		public void SetMultipleProperties_EventArgsAlloc()
		{
			var entry = new Entry();
			for (int i = 0; i < 500; i++)
			{
				entry.Text = "a";
				entry.Placeholder = "p";
				entry.FontSize = 14 + (i % 3);
				entry.Text = "b";
				entry.Placeholder = "q";
			}
		}

		// --- #34093: Reuse ElementEventArgs in tree propagation ---

		/// <summary>
		/// Adds children to a deep hierarchy, triggering DescendantAdded propagation.
		/// Before: new ElementEventArgs at every tree level.
		/// After: single ElementEventArgs reused through recursion.
		/// </summary>
		[Benchmark]
		public void AddChildren_DeepTree_ElementEventArgs()
		{
			// Build a 10-level deep tree
			var root = new VerticalStackLayout();
			var current = root;
			for (int depth = 0; depth < 10; depth++)
			{
				var child = new VerticalStackLayout();
				current.Add(child);
				current = child;
			}

			// Add 100 leaves at the bottom — each fires DescendantAdded 10 levels up
			for (int i = 0; i < 100; i++)
			{
				current.Add(new Label());
			}
		}

		/// <summary>
		/// Removes children from a deep hierarchy, triggering DescendantRemoved propagation.
		/// </summary>
		[Benchmark]
		public void RemoveChildren_DeepTree_ElementEventArgs()
		{
			var root = new VerticalStackLayout();
			var current = root;
			for (int depth = 0; depth < 10; depth++)
			{
				var child = new VerticalStackLayout();
				current.Add(child);
				current = child;
			}

			var labels = new Label[100];
			for (int i = 0; i < 100; i++)
			{
				labels[i] = new Label();
				current.Add(labels[i]);
			}

			for (int i = 0; i < 100; i++)
			{
				current.Remove(labels[i]);
			}
		}

		// --- #34129: BindingContext propagation (WeakReference reuse + .ToArray() elimination) ---

		class SimpleViewModel
		{
			public string Name { get; set; } = "Test";
		}

		/// <summary>
		/// Sets BindingContext on a flat layout with many children.
		/// Before: new WeakReference per child + .ToArray() on each child's _properties.
		/// After: reuse WeakReference.Target + foreach on dictionary directly.
		/// </summary>
		[Benchmark]
		public void SetBindingContext_FlatTree()
		{
			var layout = new VerticalStackLayout();
			for (int i = 0; i < 200; i++)
			{
				layout.Add(new Label());
			}

			var vm = new SimpleViewModel();
			for (int i = 0; i < 10; i++)
			{
				layout.BindingContext = vm;
				layout.BindingContext = null;
			}
		}

		/// <summary>
		/// Sets BindingContext on a deep tree where children have bindings.
		/// This is the worst-case hot path: ApplyBindings + WeakReference for every descendant.
		/// </summary>
		[Benchmark]
		public void SetBindingContext_DeepTreeWithBindings()
		{
			var root = new VerticalStackLayout();
			var current = root;

			for (int depth = 0; depth < 5; depth++)
			{
				var child = new VerticalStackLayout();
				current.Add(child);
				current = child;
			}

			for (int i = 0; i < 50; i++)
			{
				var label = new Label();
				label.SetBinding(Label.TextProperty, new Binding("Name"));
				current.Add(label);
			}

			var vm = new SimpleViewModel();
			for (int i = 0; i < 20; i++)
			{
				root.BindingContext = vm;
				root.BindingContext = null;
			}
		}

		// --- #34131: Lazy _triggerSpecificity dictionary ---

		/// <summary>
		/// Creates many BindableObjects (Labels) that never use triggers.
		/// Before: each allocates Dictionary&lt;TriggerBase, SetterSpecificity&gt;.
		/// After: dictionary is null until first trigger attachment.
		/// </summary>
		[Benchmark]
		public Label[] CreateManyLabels_NoTriggers()
		{
			var labels = new Label[1_000];
			for (int i = 0; i < 1_000; i++)
			{
				labels[i] = new Label();
			}
			return labels;
		}

		/// <summary>
		/// Creates many Entries (more complex BindableObject) without triggers.
		/// </summary>
		[Benchmark]
		public Entry[] CreateManyEntries_NoTriggers()
		{
			var entries = new Entry[500];
			for (int i = 0; i < 500; i++)
			{
				entries[i] = new Entry();
			}
			return entries;
		}
	}
}
