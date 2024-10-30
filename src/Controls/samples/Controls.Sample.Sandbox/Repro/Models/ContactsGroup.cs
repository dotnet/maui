namespace AllTheLists.Models
{
    public class ContactsGroup : List<Contact>
    {
        public string GroupName { get; set; }

        public ContactsGroup(string groupName, List<Contact> contacts) : base(contacts)
        {
            GroupName = groupName;
        }
    }
}