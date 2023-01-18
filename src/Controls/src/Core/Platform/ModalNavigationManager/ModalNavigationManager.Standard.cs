using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Platform
{
	internal partial class ModalNavigationManager
	{
		public Task<Page> PopModalAsync(bool animated)
		{
			if (ModalStack.Count == 0)
				throw new InvalidOperationException();

			return Task.FromResult(_navModel.PopModal());
		}

		public Task PushModalAsync(Page modal, bool animated)
		{
			_navModel.PushModal(modal);
			return Task.CompletedTask;
		}
	}
}
