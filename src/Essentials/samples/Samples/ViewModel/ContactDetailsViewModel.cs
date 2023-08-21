// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.ApplicationModel.Communication;

namespace Samples.ViewModel
{
	class ContactDetailsViewModel : BaseViewModel
	{
		public ContactDetailsViewModel(Contact contact)
		{
			Contact = contact;
		}

		public Contact Contact { get; }
	}
}
