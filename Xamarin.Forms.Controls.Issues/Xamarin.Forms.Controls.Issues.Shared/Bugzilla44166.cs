using System;
using System.Collections.ObjectModel;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Threading;
using System.Diagnostics;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 44166, "MasterDetailPage instances do not get disposed upon GC")]
	public class Bugzilla44166 : TestContentPage
	{
		protected override void Init()
		{
			var label = new Label() { Text = "Testing..." };

			var goButton = new Button { Text = "Go", AutomationId = "Go" };
			goButton.Clicked += (sender, args) => Application.Current.MainPage = new _44166MDP();

			var gcButton = new Button { Text = "GC", AutomationId = "GC" };
			gcButton.Clicked += (sender, args) =>
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();

				if (_44166MDP.Counter > 0)
				{
					Debug.WriteLine($">>>>>>>> Post-GC, {_44166MDP.Counter} {nameof(_44166MDP)} allocated");
				}

				if (_44166Master.Counter > 0)
				{
					Debug.WriteLine($">>>>>>>> Post-GC, {_44166Master.Counter} {nameof(_44166Master)} allocated");
				}

				if (_44166Detail.Counter > 0)
				{
					Debug.WriteLine($">>>>>>>> Post-GC, {_44166Detail.Counter} {nameof(_44166Detail)} allocated");
				}

				if (_44166NavContent.Counter > 0)
				{
					Debug.WriteLine($">>>>>>>> Post-GC, {_44166NavContent.Counter} {nameof(_44166NavContent)} allocated");
				}

				int success = 0;

				//some reason there's always 1 instance around i don't know why yet, if we were leaking it should be 8 here
				if (Device.RuntimePlatform == Device.macOS)
					success = 4;

				if (_44166NavContent.Counter + _44166Detail.Counter + _44166Master.Counter + _44166MDP.Counter == success)
				{
					label.Text = "Success";
				}
			};

			Content = new StackLayout
			{
				Children = { label, goButton, gcButton }
			};
		}

#if UITEST
		[Test]
		public void Bugzilla44166Test()
		{
			RunningApp.WaitForElement(q => q.Marked("Go"));
			RunningApp.Tap(q => q.Marked("Go"));

			RunningApp.WaitForElement(q => q.Marked("Previous"));
			RunningApp.Tap(q => q.Marked("Previous"));

			RunningApp.WaitForElement(q => q.Marked("GC"));

			for (var n = 0; n < 10; n++)
			{
				RunningApp.Tap(q => q.Marked("GC"));

				if (RunningApp.Query(q => q.Marked("Success")).Length > 0)
				{
					return;
				}
			}

			string pageStats = string.Empty;

			if (_44166MDP.Counter > 0)
			{
				pageStats += $"{_44166MDP.Counter} {nameof(_44166MDP)} allocated; ";
			}

			if (_44166Master.Counter > 0)
			{
				pageStats += $"{_44166Master.Counter} {nameof(_44166Master)} allocated; ";
			}

			if (_44166Detail.Counter > 0)
			{
				pageStats += $"{_44166Detail.Counter} {nameof(_44166Detail)} allocated; ";
			}

			if (_44166NavContent.Counter > 0)
			{
				pageStats += $"{_44166NavContent.Counter} {nameof(_44166NavContent)} allocated; ";
			}

			Assert.Fail($"At least one of the pages was not collected: {pageStats}");
		}
#endif
	}

	[Preserve(AllMembers = true)]
	public class _44166MDP : MasterDetailPage
	{
		public static int Counter;

		public _44166MDP()
		{
			Interlocked.Increment(ref Counter);
			Debug.WriteLine($"++++++++ {nameof(_44166MDP)} constructor, {Counter} allocated");

			Master = new _44166Master();
			Detail = new _44166Detail();
        }

		~_44166MDP()
		{
			Interlocked.Decrement(ref Counter);
			Debug.WriteLine($"-------- {nameof(_44166MDP)} destructor, {Counter} allocated");
		}
	}

	[Preserve(AllMembers = true)]
	public class _44166Master : ContentPage
	{
		public static int Counter;

		public _44166Master()
		{
			Interlocked.Increment(ref Counter);
			Debug.WriteLine($"++++++++ {nameof(_44166Master)} constructor, {Counter} allocated");

			Title = "Master";
			var goButton = new Button { Text = "Return", AutomationId = "Return"};
			goButton.Clicked += (sender, args) => Application.Current.MainPage = new Bugzilla44166();

			Content = new StackLayout
			{
				Children = { goButton }
			};
		}

		~_44166Master()
		{
			Interlocked.Decrement(ref Counter);
			Debug.WriteLine($"-------- {nameof(_44166Master)} destructor, {Counter} allocated");
		}
	}

	[Preserve(AllMembers = true)]
	public class _44166Detail : NavigationPage
	{
		public static int Counter;

		public _44166Detail()
		{
			Interlocked.Increment(ref Counter);
			Debug.WriteLine($"++++++++ {nameof(_44166Detail)} constructor, {Counter} allocated");

			Title = "Detail";
			PushAsync(new _44166NavContent());
		}

		~_44166Detail()
		{
			Interlocked.Decrement(ref Counter);
			Debug.WriteLine($"-------- {nameof(_44166Detail)} destructor, {Counter} allocated");
		}
	}

	[Preserve(AllMembers = true)]
	public class _44166NavContent : ContentPage
	{
		public static int Counter;

		public _44166NavContent()
		{
			Interlocked.Increment(ref Counter);
			Debug.WriteLine($"++++++++ {nameof(_44166NavContent)} constructor, {Counter} allocated");

			var goButton = new Button { Text = "Previous", AutomationId = "Previous" };
			goButton.Clicked += (sender, args) => Application.Current.MainPage = new Bugzilla44166();

			Content = new StackLayout
			{
				Children = { goButton }
			};
		}

		~_44166NavContent()
		{
			Interlocked.Decrement(ref Counter);
			Debug.WriteLine($"-------- {nameof(_44166NavContent)} destructor, {Counter} allocated");
		}
	}
}