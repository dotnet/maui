// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xamarin.UITest;

namespace TestUtils.Appium.UITests
{
	public interface IApp2 : IApp, IDisposable
	{
		void ActivateApp();
		void CloseApp();
		string ElementTree { get; }
		ApplicationState AppState { get; }
		bool WaitForTextToBePresentInElement(string automationId, string text);
		public byte[] Screenshot();
	}
}
