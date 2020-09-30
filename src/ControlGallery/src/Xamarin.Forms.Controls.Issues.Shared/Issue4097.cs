using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using static Xamarin.Forms.DependencyService;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 4097, "Dependency service thread safety test", PlatformAffected.All)]
	public class Issue4097 : TestContentPage
	{
		private const int TasksQuantity = 3000;

		protected override void Init()
		{
			var infoLabel = new Label
			{
				TextColor = Color.Black,
				VerticalOptions = LayoutOptions.CenterAndExpand,
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				IsVisible = false
			};

			Content = new StackLayout
			{
				Children = {
					infoLabel,
					new Button
					{
						BackgroundColor = Color.Black,
						TextColor = Color.White,
						Text = "START TEST",
						Command = new Command(async () => {
							var parentChildren = (infoLabel.Parent as StackLayout).Children;
							parentChildren.Remove(parentChildren[1]);
							infoLabel.IsVisible = true;

							Register<Test1>();
							Register<Test2>();
							Register<Test3>();
							Register<Test4>();
							Register<Test5>();
							Register<Test6>();
							Register<Test7>();
							Register<Test8>();
							Register<Test9>();
							Register<Test10>();

							Action<int> getAction = (i) => {
								switch(i % 10 + 1)
								{
									case 1:
										DependencyService.Get<ITest1>();
										break;
									case 2:
										DependencyService.Get<ITest2>();
										break;
									case 3:
										DependencyService.Get<ITest3>();
										break;
									case 4:
										DependencyService.Get<ITest4>();
										break;
									case 5:
										DependencyService.Get<ITest5>();
										break;
									case 6:
										DependencyService.Get<ITest6>();
										break;
									case 7:
										DependencyService.Get<ITest7>();
										break;
									case 8:
										DependencyService.Get<ITest8>();
										break;
									case 9:
										DependencyService.Get<ITest9>();
										break;
									case 10:
										DependencyService.Get<ITest10>();
										break;
								}
							};

							var tasks = Enumerable.Range(0, TasksQuantity).Select(i => new Task(() => getAction(i))).ToArray();
							try
							{
								foreach(var t in tasks)
								{
									t.Start();
								}
								await Task.WhenAll(tasks).ConfigureAwait(false);
							}
							catch
							{
								Device.BeginInvokeOnMainThread(() => infoLabel.Text = $"GOT EXCEPTION! FATAL");
								return;
							}
							Device.BeginInvokeOnMainThread(() => infoLabel.Text = $"TASKS QUANTITY {tasks.Length}\nSUCCESS {tasks.Count(x => !x.IsFaulted)}\nFAILED {tasks.Count(x => x.IsFaulted)}");
						})
					}
				}
			};
		}

		public interface ITest1 { }
		public interface ITest2 { }
		public interface ITest3 { }
		public interface ITest4 { }
		public interface ITest5 { }
		public interface ITest6 { }
		public interface ITest7 { }
		public interface ITest8 { }
		public interface ITest9 { }
		public interface ITest10 { }

		[Preserve(AllMembers = true)]
		public class Test1 : ITest1 { }
		[Preserve(AllMembers = true)]
		public class Test2 : ITest2 { }
		[Preserve(AllMembers = true)]
		public class Test3 : ITest3 { }
		[Preserve(AllMembers = true)]
		public class Test4 : ITest4 { }
		[Preserve(AllMembers = true)]
		public class Test5 : ITest5 { }
		[Preserve(AllMembers = true)]
		public class Test6 : ITest6 { }
		[Preserve(AllMembers = true)]
		public class Test7 : ITest7 { }
		[Preserve(AllMembers = true)]
		public class Test8 : ITest8 { }
		[Preserve(AllMembers = true)]
		public class Test9 : ITest9 { }
		[Preserve(AllMembers = true)]
		public class Test10 : ITest10 { }
	}
}