using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public class MauiWindow : Window
	{
		public MauiWindow() : base() { }
		public MauiWindow(Page page) : base(page) { }

		protected override void OnCreated()
		{
			Console.WriteLine("OnCreated");
			base.OnCreated();
		}

		protected override void OnStopped()
		{
			Console.WriteLine("OnStopped");
			base.OnStopped();
		}

		protected override void OnResumed()
		{
			Console.WriteLine("OnResumed");
			base.OnResumed();
		}

		protected override void OnDestroying()
		{
			Console.WriteLine("OnDestroying");
			base.OnDestroying();
		}
	}
}