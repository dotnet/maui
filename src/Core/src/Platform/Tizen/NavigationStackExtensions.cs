using System;
using System.Threading.Tasks;
using Tizen.UIExtensions.NUI;
using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Platform
{
	public static class NavigationStackExtensions
	{
		public static async Task PushDummyPopupPage(this NavigationStack stack, Func<Task> openPopup)
		{
			var dummy = new NView();
			stack.ShownBehindPage = true;
			_ = stack.Push(dummy, false);
			stack.ShownBehindPage = false;
			try
			{
				await openPopup();
			}
			catch
			{
				// ignore all exceptions
			}
			finally
			{
				if (stack.Top == dummy)
				{
					_ = stack.Pop(false);
				}
				else
				{
					stack.Pop(dummy);
				}
			}
		}
	}
}