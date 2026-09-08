using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class AppThemeBindingApplyTests : BaseTestFixture
	{
		// The dispatch entry is dormant today. An open delegate exercises it without
		// MethodInfo.Invoke wrapping the exceptions whose behavior these tests preserve.
		static readonly Action<AppThemeBinding, bool> ApplyCore = typeof(AppThemeBinding)
			.GetMethod("ApplyCore", BindingFlags.Instance | BindingFlags.NonPublic)
			.CreateDelegate<Action<AppThemeBinding, bool>>();

		readonly Application _app;

		public AppThemeBindingApplyTests()
		{
			AppInfo.SetCurrent(new MockAppInfo { RequestedTheme = AppTheme.Light });
			Application.Current = _app = new Application();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				Application.Current = null;

			base.Dispose(disposing);
		}

		[Fact]
		public void EqualApplyStillValidatesAndCoercesWithoutDuplicateNotifications()
		{
			var calls = new List<string>();
			var notifications = 0;
			var property = BindableProperty.Create("ObservedTheme", typeof(string), typeof(ThemeTarget), null,
				validateValue: (_, value) =>
				{
					calls.Add($"validate:{value}");
					return true;
				},
				coerceValue: (_, value) =>
				{
					calls.Add($"coerce:{value}");
					return ((string)value).ToUpperInvariant();
				},
				propertyChanging: (_, _, _) => notifications++,
				propertyChanged: (_, _, _) => notifications++);
			var target = new ThemeTarget();
			var binding = new AppThemeBinding { Light = "light" };
			target.SetBinding(property, binding);
			Assert.Equal("LIGHT", target.GetValue(property));

			target.PropertyChanging += (_, _) => notifications++;
			target.PropertyChanged += (_, _) => notifications++;
			binding.Light = new string("LIGHT".ToCharArray());
			calls.Clear();
			notifications = 0;

			binding.Apply(false);
			binding.Apply(false);

			Assert.Equal(new[] { "validate:LIGHT", "coerce:LIGHT", "validate:LIGHT", "coerce:LIGHT" }, calls);
			Assert.Equal(0, notifications);
			Assert.Equal("LIGHT", target.GetValue(property));
		}

		[Fact]
		public void ApplyConvertsValuesAndLeavesPreviousValueOnConversionFailure()
		{
			var property = BindableProperty.Create("Number", typeof(int), typeof(ThemeTarget), 0);
			var target = new ThemeTarget();
			var binding = new AppThemeBinding { Light = "12" };
			binding.Apply(null, target, property, false, SetterSpecificity.FromBinding);
			Assert.Equal(12, target.GetValue(property));
			var changed = 0;
			target.PropertyChanged += (_, _) => changed++;

			binding.Light = "34";
			binding.Apply(false);
			Assert.Equal(34, target.GetValue(property));
			Assert.Equal(1, changed);

			binding.Light = "not a number";
			binding.Apply(false);
			Assert.Equal(34, target.GetValue(property));
			Assert.Equal(1, changed);
		}

		[Fact]
		public void ApplyUsesCurrentDefaultButDoesNotReplaceExplicitNull()
		{
			var target = new ThemeTarget();
			var binding = new AppThemeBinding { Default = "first default", Dark = null };
			target.SetBinding(ThemeTarget.ThemeProperty, binding);
			Assert.Equal("first default", target.Theme);

			binding.Default = "second default";
			binding.Apply(false);
			Assert.Equal("second default", target.Theme);

			_app.UserAppTheme = AppTheme.Dark;
			binding.Apply(false);
			Assert.Null(target.Theme);

			_app.UserAppTheme = AppTheme.Light;
			binding.Apply(false);
			Assert.Equal("second default", target.Theme);
		}

		[Fact]
		public void ApplyUpdatesDynamicResourceAndPreservesLiteralRegistrationBehavior()
		{
			var target = new Label
			{
				Resources = new ResourceDictionary
				{
					{ "first", "first value" },
					{ "second", "second value" }
				}
			};
			var binding = new AppThemeBinding { Light = new DynamicResource("first") };
			binding.Apply(null, target, Label.TextProperty, false, SetterSpecificity.FromBinding);
			Assert.Equal("first value", target.Text);
			target.Resources["first"] = "first updated";
			Assert.Equal("first updated", target.Text);

			binding.Light = new DynamicResource("second");
			binding.Apply(false);
			Assert.Equal("second value", target.Text);
			target.Resources["first"] = "stale first";
			Assert.Equal("second value", target.Text);
			target.Resources["second"] = "second updated";
			Assert.Equal("second updated", target.Text);

			var changed = 0;
			target.PropertyChanged += (_, args) =>
			{
				if (args.PropertyName == nameof(Label.Text))
					changed++;
			};
			binding.Light = "second updated";
			binding.Apply(false);
			Assert.Equal("second updated", target.Text);
			Assert.Equal(0, changed);

			// The current resource implementation keeps this registration even
			// after a literal setter. The ApplyCore refactor must not change it.
			target.Resources["second"] = "still registered";
			Assert.Equal("still registered", target.Text);
			Assert.Equal(1, changed);
		}

		[Fact]
		public void ReentrantApplyToSamePropertyWaitsForCurrentNotifications()
		{
			var target = new ThemeTarget();
			var binding = new AppThemeBinding { Light = "initial" };
			binding.Apply(null, target, ThemeTarget.ThemeProperty, false, SetterSpecificity.FromBinding);
			var calls = new List<string>();
			target.PropertyChanging += (_, _) => calls.Add($"changing:{target.Theme}");
			target.PropertyChanged += (_, _) =>
			{
				calls.Add($"first:{target.Theme}");
				if (target.Theme == "outer")
				{
					binding.Light = "inner";
					binding.Apply(false);
					calls.Add($"returned:{target.Theme}");
				}
			};
			target.PropertyChanged += (_, _) => calls.Add($"second:{target.Theme}");

			binding.Light = "outer";
			binding.Apply(false);

			Assert.Equal(new[]
			{
				"changing:initial", "first:outer", "returned:outer", "second:outer",
				"changing:outer", "first:inner", "second:inner"
			}, calls);
			Assert.Equal("inner", target.Theme);
		}

		[Fact]
		public void QueuedApplyReadsThemeAndValueWhenCallbackExecutes()
		{
			var callbacks = new Queue<Action>();
			UseDispatcher(() => true, callbacks.Enqueue);
			var target = new ThemeTarget();
			var binding = new AppThemeBinding { Light = "light", Dark = "old dark" };
			target.SetBinding(ThemeTarget.ThemeProperty, binding);

			ApplyCore(binding, true);
			Assert.Single(callbacks);
			_app.UserAppTheme = AppTheme.Dark;
			binding.Dark = "new dark";
			Assert.Equal("light", target.Theme);

			callbacks.Dequeue()();

			Assert.Equal("new dark", target.Theme);
			Assert.Empty(callbacks);
		}

		[Fact]
		public void QueuedApplyAfterUnapplyPreservesNullPropertyException()
		{
			var callbacks = new Queue<Action>();
			UseDispatcher(() => true, callbacks.Enqueue);
			var target = new ThemeTarget();
			var binding = new AppThemeBinding { Light = "initial" };
			binding.Apply(null, target, ThemeTarget.ThemeProperty, false, SetterSpecificity.FromBinding);
			ApplyCore(binding, true);
			Assert.Single(callbacks);

			binding.Unapply();
			binding.Light = "queued";

			// Unapply clears _targetProperty. The original callback is not cancelled
			// and dereferences that null property; preserving this is intentional.
			Assert.Throws<NullReferenceException>(callbacks.Dequeue());
			Assert.Equal("initial", target.Theme);
			Assert.Empty(callbacks);
		}

		[Fact]
		public void QueuedApplyUsesOriginalTargetButRetargetedPropertySpecificityAndApp()
		{
			var callbacks = new Queue<Action>();
			UseDispatcher(() => true, callbacks.Enqueue);
			var original = new ContentPage();
			var replacement = new ContentPage();
			_app.LoadPage(original);
			var otherApp = new Application(false) { UserAppTheme = AppTheme.Dark };
			otherApp.LoadPage(replacement);
			Assert.Same(_app, Application.Current);
			Assert.Same(_app, original.Window.Parent);
			Assert.Same(otherApp, replacement.Window.Parent);

			var coercedTargets = new List<BindableObject>();
			var numberProperty = BindableProperty.CreateAttached("Number", typeof(int), typeof(AppThemeBindingApplyTests), 0,
				coerceValue: (target, value) =>
				{
					coercedTargets.Add(target);
					return (int)value + 1;
				});
			original.SetValue(numberProperty, 100);
			var binding = new AppThemeBinding { Light = "10", Dark = "20" };
			binding.Apply(null, original, Page.TitleProperty, false, SetterSpecificity.FromBinding);
			ApplyCore(binding, true);
			Assert.Single(callbacks);

			binding.Unapply();
			binding.Apply(null, replacement, numberProperty, false, SetterSpecificity.Trigger);
			Assert.Equal(21, replacement.GetValue(numberProperty));
			binding.Dark = "40";
			coercedTargets.Clear();
			Assert.Equal(101, original.GetValue(numberProperty));

			callbacks.Dequeue()();

			// The queued target is strong and fixed, but property, specificity and
			// GetValue's weak-target/app lookup all belong to the current binding.
			Assert.Equal("10", original.Title);
			Assert.Equal(41, original.GetValue(numberProperty));
			Assert.Equal(21, replacement.GetValue(numberProperty));
			Assert.Same(original, Assert.Single(coercedTargets));
			Assert.Empty(callbacks);

			// Current Trigger specificity won over the original target's manual
			// value; the old FromBinding specificity would have lost to it.
			original.ClearValue(numberProperty, SetterSpecificity.Trigger);
			Assert.Equal(101, original.GetValue(numberProperty));
		}

		[Fact]
		public void SynchronousApplyDoesNotConsultDispatcher()
		{
			UseDispatcher(
				() => throw new InvalidOperationException("Synchronous apply must not query the dispatcher."),
				_ => throw new InvalidOperationException("Synchronous apply must not dispatch."));
			var target = new ThemeTarget();
			var binding = new AppThemeBinding { Light = "initial" };
			binding.Apply(null, target, ThemeTarget.ThemeProperty, false, SetterSpecificity.FromBinding);

			binding.Light = "updated";
			binding.Apply(false);

			Assert.Equal("updated", target.Theme);
		}

		[Fact]
		public void DispatchEntryAppliesInlineWhenDispatchIsNotRequired()
		{
			var checks = 0;
			var callbacks = new Queue<Action>();
			UseDispatcher(() =>
			{
				checks++;
				return false;
			}, callbacks.Enqueue);
			var target = new ThemeTarget();
			var binding = new AppThemeBinding { Light = "initial" };
			binding.Apply(null, target, ThemeTarget.ThemeProperty, false, SetterSpecificity.FromBinding);

			binding.Light = "updated";
			ApplyCore(binding, true);

			Assert.Equal("updated", target.Theme);
			Assert.Equal(1, checks);
			Assert.Empty(callbacks);
		}

		[Fact]
		public void DispatchEntryPropagatesDispatcherException()
		{
			var expected = new InvalidOperationException("Dispatcher rejected callback.");
			UseDispatcher(() => true, _ => throw expected);
			var target = new ThemeTarget();
			var binding = new AppThemeBinding { Light = "initial" };
			binding.Apply(null, target, ThemeTarget.ThemeProperty, false, SetterSpecificity.FromBinding);
			binding.Light = "updated";

			var actual = Assert.Throws<InvalidOperationException>(() => ApplyCore(binding, true));

			Assert.Same(expected, actual);
			Assert.Equal("initial", target.Theme);
		}

		[Fact]
		public void ApplyWithoutTargetDoesNotDispatch()
		{
			var checks = 0;
			var callbacks = new Queue<Action>();
			UseDispatcher(() =>
			{
				checks++;
				return true;
			}, callbacks.Enqueue);
			var binding = new AppThemeBinding { Light = "initial" };

			ApplyCore(binding, true);
			binding.Apply(false);

			var target = new ThemeTarget();
			binding.Apply(null, target, ThemeTarget.ThemeProperty, false, SetterSpecificity.FromBinding);
			binding.Unapply();
			binding.Light = "must not apply";
			ApplyCore(binding, true);
			binding.Apply(false);

			Assert.Equal("initial", target.Theme);
			Assert.Equal(0, checks);
			Assert.Empty(callbacks);
		}

		[Fact]
		public void ApplyWithDeadWeakTargetDoesNotDispatch()
		{
			var checks = 0;
			var callbacks = new Queue<Action>();
			UseDispatcher(() =>
			{
				checks++;
				return true;
			}, callbacks.Enqueue);
			var binding = new AppThemeBinding { Light = "initial" };
			var reference = CreateWeakTarget(binding, queueAndUnapply: false);

			Collect();
			Assert.False(IsAlive(reference));
			ApplyCore(binding, true);
			binding.Apply(false);

			Assert.Equal(0, checks);
			Assert.Empty(callbacks);
			GC.KeepAlive(binding);
		}

		[Fact]
		public void QueuedCallbackAloneRetainsUnappliedTargetUntilDrained()
		{
			var callbacks = new Queue<Action>();
			UseDispatcher(() => true, callbacks.Enqueue);
			var binding = new AppThemeBinding { Light = "initial" };
			var reference = CreateWeakTarget(binding, queueAndUnapply: true);
			// Assert.Single returns the callback, extending its lifetime in Debug.
			Assert.Collection(callbacks, _ => { });

			Collect();
			Assert.True(IsAlive(reference));

			DrainUnappliedCallback(callbacks);
			Assert.Empty(callbacks);
			Collect();

			Assert.False(IsAlive(reference));
			GC.KeepAlive(binding);
			GC.KeepAlive(callbacks);
		}

		[Fact]
		public void SynchronousApplyDoesNotAllocate()
		{
			var target = new ThemeTarget();
			var binding = new AppThemeBinding { Light = "light", Dark = "dark" };
			target.SetBinding(ThemeTarget.ThemeProperty, binding);
			Assert.Equal("light", target.Theme);

			for (var i = 0; i < 256; i++)
				binding.Apply(false);

			var before = GC.GetAllocatedBytesForCurrentThread();
			for (var i = 0; i < 512; i++)
				binding.Apply(false);
			var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

			Assert.Equal(0L, allocated);
			GC.KeepAlive(target);
			GC.KeepAlive(binding);
		}

		static void UseDispatcher(Func<bool> isRequired, Action<Action> dispatch)
		{
			// Do not mutate DispatcherProviderStubOptions: each target captures this
			// fixture-local dispatcher when it is constructed.
			DispatcherProvider.SetCurrent(new TestDispatcherProvider(new DispatcherStub(isRequired, dispatch)));
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference<ThemeTarget> CreateWeakTarget(AppThemeBinding binding, bool queueAndUnapply)
		{
			// A plain BindableObject avoids AppThemeProxy's strong Element parent.
			var target = new ThemeTarget();
			binding.Apply(null, target, ThemeTarget.ThemeProperty, false, SetterSpecificity.FromBinding);
			if (queueAndUnapply)
			{
				ApplyCore(binding, true);
				binding.Unapply();
			}

			return new WeakReference<ThemeTarget>(target);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static bool IsAlive(WeakReference<ThemeTarget> reference) => reference.TryGetTarget(out _);

		[MethodImpl(MethodImplOptions.NoInlining)]
		static void DrainUnappliedCallback(Queue<Action> callbacks)
		{
			// Keep both the dequeued delegate and its exception off the GC test's stack.
			Assert.Throws<NullReferenceException>(callbacks.Dequeue());
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static void Collect()
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
		}

		sealed class TestDispatcherProvider : IDispatcherProvider
		{
			readonly IDispatcher _dispatcher;

			public TestDispatcherProvider(IDispatcher dispatcher) => _dispatcher = dispatcher;

			public IDispatcher GetForCurrentThread() => _dispatcher;
		}

		sealed class ThemeTarget : BindableObject
		{
			public static readonly BindableProperty ThemeProperty =
				BindableProperty.Create(nameof(Theme), typeof(string), typeof(ThemeTarget), null);

			public string Theme => (string)GetValue(ThemeProperty);
		}
	}
}
