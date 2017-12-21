using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.WPF;
using Xamarin.Forms.Controls;

[assembly: Dependency(typeof(StringProvider))]
namespace Xamarin.Forms.ControlGallery.WPF
{
	public class StringProvider : IStringProvider
	{
		public string CoreGalleryTitle
		{
			get { return "WPF Core Gallery"; }
		}
	}
}
