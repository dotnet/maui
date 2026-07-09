using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers.Benchmarks
{
	[MemoryDiagnoser]
	[Orderer(SummaryOrderPolicy.FastestToSlowest)]
	public class ParentResourceRefreshBenchmarker
	{
		const int Operations = 100;

		// Scale unrelated app resources so the results show whether attach cost tracks resource payload size.
		[Params(50, 200, 1000, 2000)]
		public int AppResourceCount { get; set; }

		Application _previousApplication;
		Application _application;
		VerticalStackLayout _host;
		View[] _leafViewsWithoutResourceListener;
		View[] _leafViewsWithResourceListener;
		View[] _subtreeRootViewsWithResourceListener;

		[GlobalSetup]
		public void Setup()
		{
			_previousApplication = Application.Current;
			_application = new Application
			{
				Resources = CreateAppResources(AppResourceCount),
			};
			Application.Current = _application;

			var page = CreateComplexPage(out _host);
			page.Parent = _application;

			_leafViewsWithoutResourceListener = CreateLeafViews(withResourceListener: false);
			_leafViewsWithResourceListener = CreateLeafViews(withResourceListener: true);
			_subtreeRootViewsWithResourceListener = CreateSubtreeRootViewsWithResourceListener();
		}

		[GlobalCleanup]
		public void Cleanup()
		{
			Application.Current = _previousApplication;
		}

		[Benchmark(OperationsPerInvoke = Operations)]
		public void AttachLeafViews_NoResourceListener()
		{
			AttachAndDetachAll(_leafViewsWithoutResourceListener);
		}

		[Benchmark(OperationsPerInvoke = Operations)]
		public void AttachLeafViews_WithResourceListener()
		{
			AttachAndDetachAll(_leafViewsWithResourceListener);
		}

		[Benchmark(OperationsPerInvoke = Operations)]
		public void AttachSubtreeRoots_WithResourceListener()
		{
			AttachAndDetachAll(_subtreeRootViewsWithResourceListener);
		}

		// Batch variants report the full 100-operation burst, which makes cumulative allocation
		// differences easier to compare with app-level attach bursts.
		[Benchmark]
		public void AttachLeafViews_NoResourceListener_Batch()
		{
			AttachAndDetachAll(_leafViewsWithoutResourceListener);
		}

		[Benchmark]
		public void AttachLeafViews_WithResourceListener_Batch()
		{
			AttachAndDetachAll(_leafViewsWithResourceListener);
		}

		[Benchmark]
		public void AttachSubtreeRoots_WithResourceListener_Batch()
		{
			AttachAndDetachAll(_subtreeRootViewsWithResourceListener);
		}

		void AttachAndDetachAll(View[] views)
		{
			for (var i = 0; i < Operations; i++)
				AttachAndDetach(views[i]);
		}

		void AttachAndDetach(View view)
		{
			_host.AddLogicalChild(view);
			_host.RemoveLogicalChild(view);
		}

		static ResourceDictionary CreateAppResources(int appResourceCount)
		{
			var resources = new ResourceDictionary();
			for (var i = 0; i < appResourceCount; i++)
				resources.Add($"app-resource-{i}", i);

			resources.Add(new Style(typeof(Label))
			{
				Setters = {
					new Setter { Property = Label.TextColorProperty, Value = Colors.Black },
					new Setter { Property = Label.FontSizeProperty, Value = 14d },
				}
			});

			resources.Add(new Style(typeof(Button))
			{
				Setters = {
					new Setter { Property = Button.TextColorProperty, Value = Colors.White },
					new Setter { Property = Button.BackgroundColorProperty, Value = Colors.DarkBlue },
				}
			});

			resources.Add(new Style(typeof(Label))
			{
				Class = "benchmark-accent",
				ApplyToDerivedTypes = true,
				Setters = {
					new Setter { Property = Label.TextColorProperty, Value = Colors.DarkGreen },
				}
			});

			resources.Add(new Style(typeof(Button))
			{
				Class = "benchmark-accent",
				ApplyToDerivedTypes = true,
				Setters = {
					new Setter { Property = Button.TextColorProperty, Value = Colors.Yellow },
				}
			});

			return resources;
		}

		static ContentPage CreateComplexPage(out VerticalStackLayout host)
		{
			host = new VerticalStackLayout
			{
				Resources = CreateLocalResources("host", 50),
			};

			var formSection = new Grid
			{
				Resources = CreateLocalResources("form-section", 50),
				Children = {
					new Label { Text = "Account", StyleClass = new[] { "benchmark-accent" } },
					new Button { Text = "Save", StyleClass = new[] { "benchmark-accent" } },
					host,
				}
			};

			var detailSection = new VerticalStackLayout
			{
				Resources = CreateLocalResources("detail-section", 50),
				Children = {
					new Label { Text = "Details" },
					formSection,
				}
			};

			var content = new Grid
			{
				Resources = CreateLocalResources("content", 50),
				Children = {
					new VerticalStackLayout
					{
						Resources = CreateLocalResources("nav", 50),
						Children = {
							new Label { Text = "Dashboard" },
							detailSection,
						}
					}
				}
			};

			return new ContentPage
			{
				Resources = CreateLocalResources("page", 50),
				Content = content,
			};
		}

		static ResourceDictionary CreateLocalResources(string prefix, int count)
		{
			var resources = new ResourceDictionary();
			for (var i = 0; i < count; i++)
				resources.Add($"{prefix}-resource-{i}", i);

			return resources;
		}

		static View[] CreateLeafViews(bool withResourceListener)
		{
			var views = new View[Operations];
			for (var i = 0; i < Operations; i++)
			{
				View view = i % 2 == 0
					? new Label { Text = $"Item {i}", StyleClass = new[] { "benchmark-accent" } }
					: new Button { Text = $"Item {i}", StyleClass = new[] { "benchmark-accent" } };

				if (withResourceListener)
					view.Background = new SolidColorBrush(Colors.Transparent);

				views[i] = view;
			}

			return views;
		}

		static View[] CreateSubtreeRootViewsWithResourceListener()
		{
			var views = new View[Operations];
			for (var i = 0; i < Operations; i++)
			{
				views[i] = new ContentView
				{
					// Brush resources listen for parent resource changes, so these roots stay on the
					// preserved full-snapshot path even though their descendants are already built.
					Background = new SolidColorBrush(Colors.Transparent),
					Resources = CreateLocalResources($"template-{i}", 10),
					Content = new Grid
					{
						Children = {
							new VerticalStackLayout
							{
								Children = {
									new Label { Text = $"Primary {i}", StyleClass = new[] { "benchmark-accent" } },
									new Label { Text = $"Secondary {i}" },
									new Button { Text = $"Action {i}", StyleClass = new[] { "benchmark-accent" } },
								}
							}
						}
					}
				};
			}

			return views;
		}
	}

	[MemoryDiagnoser]
	[Orderer(SummaryOrderPolicy.FastestToSlowest)]
	public class ParentResourceRefreshDynamicResourceBenchmarker
	{
		const int Operations = 100;
		const int AppResourceCount = 2000;

		[Params(1, 5, 20, 100, 500)]
		public int RequestedDynamicResourceCount { get; set; }

		Application _previousApplication;
		Application _application;
		VerticalStackLayout _host;
		View[] _views;

		[GlobalSetup]
		public void Setup()
		{
			_previousApplication = Application.Current;
			_application = new Application
			{
				Resources = CreateAppResources(AppResourceCount, ManyDynamicResourceView.MaxDynamicResourceCount),
			};
			Application.Current = _application;

			var page = CreateComplexPage(out _host);
			page.Parent = _application;

			_views = CreateViews(RequestedDynamicResourceCount);
		}

		[GlobalCleanup]
		public void Cleanup()
		{
			Application.Current = _previousApplication;
		}

		[Benchmark(OperationsPerInvoke = Operations)]
		public void AttachViews_MatchingDynamicResources_NoResourceListener()
		{
			AttachAndDetachAll();
		}

		[Benchmark]
		public void AttachViews_MatchingDynamicResources_NoResourceListener_Batch()
		{
			AttachAndDetachAll();
		}

		void AttachAndDetachAll()
		{
			for (var i = 0; i < Operations; i++)
			{
				var view = _views[i];
				_host.AddLogicalChild(view);
				_host.RemoveLogicalChild(view);
			}
		}

		static View[] CreateViews(int requestedDynamicResourceCount)
		{
			var views = new View[Operations];
			for (var i = 0; i < Operations; i++)
			{
				var view = new ManyDynamicResourceView();
				view.SetDynamicResources(requestedDynamicResourceCount);
				views[i] = view;
			}

			return views;
		}

		static ResourceDictionary CreateAppResources(int unrelatedResourceCount, int dynamicResourceCount)
		{
			var resources = new ResourceDictionary();
			for (var i = 0; i < unrelatedResourceCount; i++)
				resources.Add($"app-resource-{i}", i);

			for (var i = 0; i < dynamicResourceCount; i++)
				resources.Add(ManyDynamicResourceView.GetResourceKey(i), $"Value {i}");

			return resources;
		}

		static ContentPage CreateComplexPage(out VerticalStackLayout host)
		{
			host = new VerticalStackLayout
			{
				Resources = CreateLocalResources("host", 50),
			};

			var formSection = new Grid
			{
				Resources = CreateLocalResources("form-section", 50),
				Children = {
					new Label { Text = "Account" },
					host,
				}
			};

			var detailSection = new VerticalStackLayout
			{
				Resources = CreateLocalResources("detail-section", 50),
				Children = {
					new Label { Text = "Details" },
					formSection,
				}
			};

			return new ContentPage
			{
				Resources = CreateLocalResources("page", 50),
				Content = detailSection,
			};
		}

		static ResourceDictionary CreateLocalResources(string prefix, int count)
		{
			var resources = new ResourceDictionary();
			for (var i = 0; i < count; i++)
				resources.Add($"{prefix}-resource-{i}", i);

			return resources;
		}

		sealed class ManyDynamicResourceView : View
		{
			const int DynamicResourcePropertyCount = 500;
			static readonly BindableProperty[] DynamicResourceProperties = CreateDynamicResourceProperties();

			public static int MaxDynamicResourceCount => DynamicResourcePropertyCount;

			public static string GetResourceKey(int index)
			{
				return $"dynamic-resource-{index}";
			}

			public void SetDynamicResources(int count)
			{
				for (var i = 0; i < count; i++)
					SetDynamicResource(DynamicResourceProperties[i], GetResourceKey(i));
			}

			static BindableProperty[] CreateDynamicResourceProperties()
			{
				var properties = new BindableProperty[DynamicResourcePropertyCount];
				for (var i = 0; i < properties.Length; i++)
					properties[i] = BindableProperty.Create($"DynamicResourceValue{i}", typeof(object), typeof(ManyDynamicResourceView), default(object));

				return properties;
			}
		}
	}
}
