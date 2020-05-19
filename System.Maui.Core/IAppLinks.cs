using System;

namespace System.Maui
{
	public interface IAppLinks
	{
		void DeregisterLink(IAppLinkEntry appLink);
		void DeregisterLink(Uri appLinkUri);
		void RegisterLink(IAppLinkEntry appLink);
	}
}