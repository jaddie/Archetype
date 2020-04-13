using System;
using Archetype.Extensions;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

namespace Archetype.Models
{
    public class ArchetypePublishedProperty : IPublishedProperty
    {
        private readonly object _rawValue;
        private readonly Lazy<object> _sourceValue;
        private readonly Lazy<object> _objectValue;
        private readonly Lazy<object> _xpathValue;
        private readonly ArchetypePropertyModel _property;
        private readonly PublishedPropertyType _propertyType;

        public ArchetypePublishedProperty(ArchetypePropertyModel property)
        {
            if (property == null)
                throw new ArgumentNullException("property");

            var preview = false;

            _property = property;
            _rawValue = property.Value;

			// #418 - need to wrap this in a try/catch to prevent orphaned properties from breaking everything
	        try
	        {
				_propertyType = property.CreateDummyPropertyType();

				if (_propertyType != null)
				{
					_sourceValue = new Lazy<object>(() => _propertyType.ConvertSourceToInter(null,_rawValue, preview));
					_objectValue = new Lazy<object>(() => _propertyType.ConvertInterToObject(null,PropertyCacheLevel.None,_sourceValue.Value, preview));
					_xpathValue = new Lazy<object>(() => _propertyType.ConvertInterToXPath(null, PropertyCacheLevel.None,_sourceValue.Value, preview));
				}
	        }
	        catch(Exception ex)
	        {
		        Current.Logger.Warn<ArchetypePublishedProperty>(string.Format("Could not create an IPublishedProperty for property: {0} - the error was: {1}", property.Alias, ex.Message));
	        }
        }

        internal ArchetypePropertyModel ArchetypeProperty
        {
            get { return _property; }
        }

        public object DataValue
        {
            get
            {
                return _sourceValue != null
                    ? _sourceValue.Value
                    : _rawValue;
            }
        }

        public bool HasValue
        {
            get
            {
                return _rawValue != null && !string.IsNullOrEmpty(_rawValue.ToString());
            }
        }

        public string PropertyTypeAlias
        {
            get { return _property.Alias; }
        }

        public object Value
        {
            get
            {
                return _objectValue != null
                    ? _objectValue.Value
                    : _rawValue;
            }
        }

        public object XPathValue
        {
            get
            {
                return _xpathValue != null
                    ? _xpathValue.Value
                    : _rawValue;
            }
        }

        public IPublishedPropertyType PropertyType => _propertyType;

        public string Alias => _property.Alias;

        bool IPublishedProperty.HasValue(string culture, string segment)
        {
            return HasValue;
        }

        public object GetSourceValue(string culture = null, string segment = null)
        {
            return DataValue;
        }

        public object GetValue(string culture = null, string segment = null)
        {
            return Value;
        }

        public object GetXPathValue(string culture = null, string segment = null)
        {
            return XPathValue;
        }
    }
}