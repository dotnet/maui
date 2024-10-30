using System.Collections.ObjectModel;
using System.Diagnostics;
using AllTheLists.Models;
using AllTheLists.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Adapters;
using Contact = AllTheLists.Models.Contact;

namespace AllTheLists.ViewModels;

public partial class AddressBookViewModel : ObservableObject
{
    
    private List<Contact> _contacts;

    [ObservableProperty]
    private List<ContactsGroup> _contactsGroups;

    private List<ContactsGroup> _unfilteredContactsGroups;

    public AddressBookViewModel()
    {
        _contacts = MockDataService.GenerateContacts().OrderBy(c => c.LastName).ThenBy(c => c.FirstName).ToList();
        
        ContactsGroups = new List<ContactsGroup>();

        var groupedContacts = _contacts.GroupBy(c => c.LastName[0]).OrderBy(g => g.Key);

        foreach (var group in groupedContacts)
        {
            var contactsGroup = new ContactsGroup(group.Key.ToString(), group.ToList());
            ContactsGroups.Add(contactsGroup);
        }

        _unfilteredContactsGroups = new List<ContactsGroup>(ContactsGroups);
    }

    [ObservableProperty]
    private string _searchText = string.Empty;

    partial void OnSearchTextChanged(string value)
    {
        Search();
    }

    [RelayCommand]
    void Search()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            // If the search text is empty, show all contacts
            ContactsGroups = _unfilteredContactsGroups;
        }
        else
        {
            // If the search text is not empty, show only contacts that contain the search text
            ContactsGroups = _unfilteredContactsGroups
                .Select(g => new ContactsGroup(g.GroupName, g.Where(c =>
                    c.FirstName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                    || c.LastName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).ToList()))
                .Where(g => g.Any())
                .ToList();
        }
    }
}
