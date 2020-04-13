using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Umbraco.Core.PropertyEditors;

namespace Archetype.Extensions
{
    /// <summary>
    /// PropertyEditor extensions.
    /// </summary>
	public static class PropertyEditorExtensions
    {
		/// <summary>
		/// Formats the default prevalues for a property editor for use within an Archetype clientside context
		/// </summary>
		/// <param name="propertyEditor">The property editor</param>
		/// <returns>The formatted default prevalues</returns>
		public static IDictionary<string, object> DefaultPreValuesForArchetype(this IDataEditor propertyEditor)
        {
			if (propertyEditor.DefaultConfiguration == null || propertyEditor.DefaultConfiguration.Any() == false)
			{
				return propertyEditor.DefaultConfiguration;
			}
			var view = propertyEditor.GetValueEditor().View.ToLowerInvariant();

			// This is the extension point for default prevalues formatting, in case we need to handle any other
			// property editors later on. It should be replaced with a switch statement by then, or maybe some fancy 
			// auto discovery of formatters :)
			if (view == "imagecropper")
			{
				propertyEditor.FormatImageCropperDefaultPreValuesForArchetype();
			}
			return propertyEditor.DefaultConfiguration;
        }

		/// <summary>
		/// Format the default prevalues of the image cropper property editor
		/// </summary>
		/// <param name="propertyEditor"></param>
		/// <remarks>
		/// In order for the image cropper to work clientside, we need to make sure it's default prevalue "focalpoint" is returned
		/// as a JSON object and not as the string it's defined as on the image cropper property editor.
		/// </remarks>
		private static void FormatImageCropperDefaultPreValuesForArchetype(this IDataEditor propertyEditor)
		{
			const string focalPointKey = "focalPoint";

			if (propertyEditor.DefaultConfiguration.ContainsKey(focalPointKey) == false || propertyEditor.DefaultConfiguration[focalPointKey] == null)
			{
				return;
			}
			var focalPoint = propertyEditor.DefaultConfiguration[focalPointKey].ToString();
			if (string.IsNullOrEmpty(focalPoint))
			{
				return;
			}
			// translate the JSON string to a JSON object
			propertyEditor.DefaultConfiguration[focalPointKey] = JsonConvert.DeserializeObject(focalPoint);
		}
    }
}