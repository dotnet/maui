using System.Maui.CustomAttributes;
using System.Maui.Internals;
using System.Maui.Xaml;
using System;

namespace System.Maui.Controls.Issues
{
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5108, "iOS: Frame with HasShadow set to true and BackgroundColor alpha < 1 casts shadow on all child views", PlatformAffected.iOS)]
	public partial class Issue5108 : TestContentPage
	{
		public Issue5108()
		{
			InitializeComponent();
			MarginButton.Clicked += MarginButton_Clicked;
			HasShadowButton.Clicked += HasShadowButton_Clicked;
			RadiusButton.Clicked += RadiusButton_Clicked;
			BackgroundButton.Clicked += BackgroundButton_Clicked;
		}

		protected override void Init()
		{
		}


		void MarginButton_Clicked(object sender, EventArgs e)
		{
			if (myframe.Margin.Top == 20)
				myframe.Margin = new Thickness(5);
			else
				myframe.Margin = new Thickness(20);
		}

		void HasShadowButton_Clicked(object sender, EventArgs e)
		{
			myframe.HasShadow = !myframe.HasShadow;
		}

		void RadiusButton_Clicked(object sender, EventArgs e)
		{
			if (myframe.CornerRadius == 10)
				myframe.CornerRadius = 20;
			else
				myframe.CornerRadius = 10;
		}

		Color? initialColor = null;
		void BackgroundButton_Clicked(object sender, EventArgs e)
		{
			if (!initialColor.HasValue)
				initialColor = myframe.BackgroundColor;

			if (myframe.BackgroundColor == initialColor.Value)
				myframe.BackgroundColor = Color.HotPink;
			else
				myframe.BackgroundColor = initialColor.Value;
		}
	}
#endif
}
