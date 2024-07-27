using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	public partial class Issue21630_shellPage : Shell
	{
		public Issue21630_shellPage(Page page, List<Page> modalStack)
		{
			InitializeComponent();
			BindingContext = (page, modalStack);
		}
	}
}
