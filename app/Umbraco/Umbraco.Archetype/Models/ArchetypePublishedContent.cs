using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;

namespace Archetype.Models
{
    public class ArchetypePublishedContent : IPublishedElement
    {
        private readonly Dictionary<string, IPublishedProperty> _properties;

        public ArchetypePublishedContent(ArchetypeFieldsetModel fieldset, ArchetypePublishedContentSet parent = null)
        {
            if (fieldset == null)
                throw new ArgumentNullException("fieldset");

            ArchetypeFieldset = fieldset;
            ContentSet = parent ?? Enumerable.Empty<IPublishedElement>();

            _properties = fieldset.Properties
                .ToDictionary(
                    x => x.Alias,
                    x => new ArchetypePublishedProperty(x) as IPublishedProperty,
                    StringComparer.InvariantCultureIgnoreCase);
        }

        internal ArchetypeFieldsetModel ArchetypeFieldset { get; }

        public IEnumerable<IPublishedElement> Children => Enumerable.Empty<IPublishedElement>();

        public IEnumerable<IPublishedElement> ContentSet { get; }

        public IPublishedContentType ContentType => default(PublishedContentType);

        public DateTime CreateDate => DateTime.MinValue;

        public int CreatorId => default;

        public string CreatorName => default;

        public string DocumentTypeAlias => ArchetypeFieldset.Alias;

        public int DocumentTypeId => default;

        public int GetIndex() => ContentSet.IndexOf(this);

        public IPublishedProperty GetProperty(string alias, bool recurse)
        {
            IPublishedProperty property;
            return _properties.TryGetValue(alias, out property) ? property : null;
        }

        public IPublishedProperty GetProperty(string alias) => GetProperty(alias, false);

        public int Id => default;

        public bool IsDraft => ArchetypeFieldset.IsAvailable() == false;

        public PublishedItemType ItemType => PublishedItemType.Content;

        public int Level => default;

        public string Name => default;

        public IPublishedElement Parent => default;

        public string Path => default;

        public IEnumerable<IPublishedProperty> Properties => _properties.Values;

        public int SortOrder => default;

        public int TemplateId => default;

        public DateTime UpdateDate => default;

        public string Url => default;

        public string UrlName => default;

        public Guid Version => Guid.Empty;

        public int WriterId => default;

        public string WriterName => default;

        public Guid Key => Guid.Empty;

        public object this[string alias]
        {
            get
            {
                var property = GetProperty(alias);

                return property?.Value();
            }
        }
    }
}