#if !UITEST
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 6698, "crash on TypedBinding.Apply", PlatformAffected.All)]
	public class Issue6698 : TestContentPage
	{
		protected override void Init()
		{
			_container = new AbsoluteLayout();
			var button1 = new Button { Text = "Test 1 (AbsoluteLayout)" };
			button1.Clicked += Button1OnClicked;
			Grid.SetRow(button1, 1);
			var content = new Grid
			{
				RowDefinitions =
				{
					new RowDefinition { Height = GridLength.Star },
					new RowDefinition { Height = GridLength.Auto }
				},
				Children = { _container, button1 }
			};
			Content = content;
		}

		async void Button1OnClicked(object sender, EventArgs e)
		{
			// Simulation of page transition
			for (var i = 0; i < 1000; i++)
			{
				// Exception triggered by this update
				GetLongLifecycleModel().Next();

				ReplaceView(new Issue6698View2 { BindingContext = CreateContainerViewModel() });

				await Task.Delay(10);
			}

			Cleanup();
		}

		void ReplaceView(View view)
		{
			Cleanup();

			AbsoluteLayout.SetLayoutFlags(view,
				AbsoluteLayoutFlags.WidthProportional | AbsoluteLayoutFlags.HeightProportional);
			AbsoluteLayout.SetLayoutBounds(view, new Rectangle(0, 0, 1, 1));

			_container.Children.Add(view);
		}

		void Cleanup()
		{
			// If you clear the BindingContext of the old View, no problem occurs?
			//foreach (var view in Container.Children)
			//{
			//    view.BindingContext = null;
			//}

			_container.Children.Clear();
		}

		static Issue6698LongLifecycleModel _longLifecycleModel;
		AbsoluteLayout _container;

		static Issue6698LongLifecycleModel GetLongLifecycleModel() =>
			_longLifecycleModel ?? (_longLifecycleModel = new Issue6698LongLifecycleModel());

		static Issue6698ContainerViewModel CreateContainerViewModel() =>
			new Issue6698ContainerViewModel(GetLongLifecycleModel());
	}

	public class Issue6698LongLifecycleModel : INotifyPropertyChanged
	{
		private int nextId;

		public Issue6698Entity Entity { get; private set; }

		public void Next()
		{
			nextId++;
			Entity = new Issue6698Entity { Id = nextId, Name = $"Entity-{nextId}", Buffer = new byte[1024] };

			for (var i = 0; i < 80; i++)
				OnPropertyChanged(nameof(Entity));
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	public class Issue6698ContainerViewModel
	{
		public Issue6698LongLifecycleModel LongLifecycleModel { get; }

		public Issue6698ContainerViewModel(Issue6698LongLifecycleModel longLifecycle)
		{
			LongLifecycleModel = longLifecycle;
		}
	}

	public class Issue6698Entity
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public byte[] Buffer { get; set; }
	}
}
#endif