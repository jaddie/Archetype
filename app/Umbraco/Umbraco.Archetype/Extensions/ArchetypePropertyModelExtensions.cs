using Archetype.Models;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

namespace Archetype.Extensions
{
    /// <summary>
    /// ArchetypePropertyModel extensions.
    /// </summary>
    public static class ArchetypePropertyModelExtensions
    {
        /// <summary>
        /// Determines whether this instance is an archetype.
        /// </summary>
        /// <param name="prop">The property.</param>
        /// <returns></returns>
        public static bool IsArchetype(this ArchetypePropertyModel prop)
        {
            return prop.PropertyEditorAlias.InvariantEquals(Constants.PropertyEditorAlias);
        }

        /// <summary>
        /// Creates dummy property type.
        /// </summary>
        /// <param name="prop">The property.</param>
        /// <returns></returns>
        internal static PublishedPropertyType CreateDummyPropertyType(this ArchetypePropertyModel prop)
        {
            // We need to check if `PropertyValueConvertersResolver` exists,
            // otherwise `PublishedPropertyType` will throw an exception outside of the Umbraco context.; e.g. unit-tests.
            if (Current.PropertyEditors.Count < 1)
                return null;

            var dataType = Current.Services.DataTypeService.GetByEditorAlias(prop.PropertyEditorAlias).FirstOrDefault();
            //TODO: How can this be done
            return new PublishedPropertyType(prop.HostContentType, new PropertyType(new DataType (dataType.Editor) { Id = prop.DataTypeId }),null,null,null);
        }
    }
}