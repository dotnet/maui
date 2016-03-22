using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace Xamarin.Forms.Core.UITests
{
	internal sealed class StateViewContainerRemote : BaseViewContainerRemote
	{
		public StateViewContainerRemote (IApp app, Enum formsType, string platformViewType)
			: base(app, formsType, platformViewType) { }

		public void TapStateButton ()
		{
			App.Screenshot ("Before state change");
			App.Tap (q => q.Raw (StateButtonQuery));
			App.Screenshot ("After state change");
		}

		public AppResult GetStateLabel ()
		{
			return App.Query (q => q.Raw (StateLabelQuery)).First ();
		}
	}
}
