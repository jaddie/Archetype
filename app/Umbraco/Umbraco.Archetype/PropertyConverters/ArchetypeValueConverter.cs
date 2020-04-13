using System;
using Archetype.Extensions;
using Archetype.Models;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Archetype.PropertyConverters
{
    /// <summary>
    /// Default property value converter that models the JSON to a C# object.
    /// </summary>
    public class ArchetypeValueConverter : PropertyValueConverterBase
    {
        /// <summary>
        /// Gets the services.
        /// </summary>
        /// <value>
        /// The services.
        /// </value>
        public ServiceContext Services
        {
            get { return Current.Services; }
        }

        /// <summary>
        /// Determines whether the specified property type is converter for Archetype.
        /// </summary>
        /// <param name="propertyType">Type of the property.</param>
        /// <returns></returns>
        public override bool IsConverter(IPublishedPropertyType propertyType)
        {
            var isArcheTypePropertyEditor = !string.IsNullOrEmpty(propertyType.EditorAlias) 
                && propertyType.EditorAlias.Equals(Constants.PropertyEditorAlias);
            if (!isArcheTypePropertyEditor)
                return false;

            return !ArchetypeHelper.Instance.IsPropertyValueConverterOverridden(propertyType.DataType.Id);
        }

        /// <summary>
        /// Converts the data to source.
        /// </summary>
        /// <param name="propertyType">Type of the property.</param>
        /// <param name="source">The source.</param>
        /// <param name="preview">if set to <c>true</c> [preview].</param>
        /// <returns></returns>
        public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object source, bool preview)
        {
            var defaultValue = new ArchetypeModel();

            if (source == null)
                return defaultValue;

            var sourceString = source.ToString();

            if (!sourceString.DetectIsJson())
                return defaultValue;

			using (var timer = Current.ProfilingLogger.DebugDuration<ArchetypeValueConverter>(string.Format("ConvertDataToSource ({0})", propertyType != null ? propertyType.EditorAlias : "null")))
            {
                var archetype = ArchetypeHelper.Instance.DeserializeJsonToArchetype(sourceString,
                    (propertyType != null ? propertyType.DataType.Id : -1),
                    (propertyType != null ? propertyType.ContentType : null));

                return archetype;
            }
        }
    }
}