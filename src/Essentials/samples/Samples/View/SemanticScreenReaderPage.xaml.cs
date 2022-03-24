using System;
using Microsoft.Maui.Accessibility;

namespace Samples.View
{
	public partial class SemanticScreenReaderPage : BasePage
	{
		public SemanticScreenReaderPage()
		{
			InitializeComponent();
		}

		void Announce_Clicked(object sender, EventArgs e)
		{
			SemanticScreenReader.Announce("This is the announcement text");
		}
	}
}
