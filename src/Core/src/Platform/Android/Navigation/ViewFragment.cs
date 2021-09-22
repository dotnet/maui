using System;
using System.Collections.Generic;
using System.Text;
using Android.OS;
using Android.Views;
using AndroidX.Fragment.App;

namespace Microsoft.Maui
{
	class ViewFragment : Fragment
	{
		readonly View _aView;

		public ViewFragment(View aView)
		{
			_aView = aView;
			_aView.RemoveFromParent();
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			_aView.RemoveFromParent();
			return _aView;
		}
	}
}
