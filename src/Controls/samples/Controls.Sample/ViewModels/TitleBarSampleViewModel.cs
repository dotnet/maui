using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Maui.Controls.Sample.ViewModels.Base;

namespace Maui.Controls.Sample.ViewModels
{
	public class TitleBarSampleViewModel : BaseViewModel
	{
		private string _title = "";
		public string Title
		{
			get { return _title; }
			set
			{
				_title = value;
				OnPropertyChanged();
			}
		}

		private string _subtitle = "";
		public string Subtitle
		{
			get { return _subtitle; }
			set
			{
				_subtitle = value;
				OnPropertyChanged();
			}
		}

		private bool _showTitleBar = true;
		public bool ShowTitleBar
		{
			get { return _showTitleBar; }
			set
			{
				_showTitleBar = value;
				OnPropertyChanged();
			}
		}
	}
}
