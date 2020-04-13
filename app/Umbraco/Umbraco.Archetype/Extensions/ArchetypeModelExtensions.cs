using System.Collections.Generic;
using Archetype.Models;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;

namespace Archetype.Extensions
{
    public static class ArchetypeModelExtensions
    {
        public static IEnumerable<IPublishedElement> ToPublishedContentSet(this ArchetypeModel archetype)
        {
            return new ArchetypePublishedContentSet(archetype);
        }
    }
}