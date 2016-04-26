using Xamarin.Forms;
using System;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls
{
	[Preserve (AllMembers = true)]
	public partial class BehaviorsAndTriggers : ContentPage
	{	
		public BehaviorsAndTriggers ()
		{
			InitializeComponent ();
		}
	}

	[Preserve (AllMembers = true)]
	public class Throb : TriggerAction<VisualElement>
	{
		bool _horizontal;

		public Throb (bool horizontal)
		{
			_horizontal = horizontal;			
		}

		protected override async void Invoke (VisualElement sender)
		{
			for (var i=0;i<5;i++){
				await sender.TranslateTo (_horizontal ? -5:0,!_horizontal ? -5 : 0, 25);
				await sender.TranslateTo (_horizontal ? 5:0,!_horizontal ? 5: 0, 25);
			}
			await sender.TranslateTo (0, 0, 25);
		}	
	}

	[Preserve (AllMembers = true)]
	public class HThrob : Throb
	{
		[Preserve]
		public HThrob ()
			: base (true)
		{			
		}
	}

	[Preserve (AllMembers = true)]
	public class VThrob : Throb
	{
		public VThrob () : base (false)
		{			
		}
	}

	[Preserve (AllMembers = true)]
	public class StopItBehavior : Behavior<Button>
	{
		protected override void OnAttachedTo (Button bindable)
		{
			base.OnAttachedTo (bindable);
			bindable.Clicked += OnClicked;
		}

		protected override void OnDetachingFrom (Button bindable)
		{
			bindable.Clicked -= OnClicked;
			base.OnDetachingFrom (bindable);
		}

		void OnClicked (object sender, EventArgs e)
		{
			var button = (Button)sender;
			button.Text = "Don't do this again";
			button.IsEnabled = false;
		}
	}	
}