using System;

namespace Samples.Model
{
    public class SampleItem
    {
        public SampleItem(string name, Type pageType, string description)
        {
            Name = name;
            Description = description;
            PageType = pageType;
        }

        public string Name { get; }

        public string Description { get; }

        public Type PageType { get; }
    }
}
