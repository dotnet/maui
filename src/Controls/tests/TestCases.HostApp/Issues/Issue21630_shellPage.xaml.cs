using Microsoft.Maui.Controls;
using System.Collections.Generic;

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
