using System;
using System.Collections.Generic;
using System.Linq;
using Archetype.Extensions;
using Archetype.Models;
using ClientDependency.Core;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.PropertyEditors;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Core.Composing;

namespace Archetype.PropertyEditors
{
    /// <summary>
    /// C# representation of the property editor. This is often done with package manifest instead.
    /// </summary>
	[PropertyEditorAsset(ClientDependencyType.Javascript, "../App_Plugins/Archetype/js/archetype.js")]
	[DataEditor(Constants.PropertyEditorAlias, "Archetype", "../App_Plugins/Archetype/views/archetype.html", ValueType = "JSON")]
	public class ArchetypePropertyEditor : DataEditor
	{
		public ArchetypePropertyEditor() : base(Current.Logger)
		{

		}
		#region Pre Value Editor

        /// <summary>
        /// Creates a pre value editor instance
        /// </summary>
        /// <returns></returns>
		protected override IConfigurationEditor CreateConfigurationEditor()
		{
			return new ArchetypePreValueEditor();
		}

        /// <summary>
        /// Class that represents the prevalue editor. This is often done with a package manifest instead.
        /// </summary>
		internal class ArchetypePreValueEditor : ConfigurationEditor
		{
			[ConfigurationField("archetypeConfig", "Config", "../App_Plugins/Archetype/views/archetype.config.html",
				Description = "(Required) Describe your Archetype.")]
			public string Config { get; set; }

			[ConfigurationField("hideLabel", "Hide Label", "boolean",
				Description = "Hide the Umbraco property title and description, making the Archetype span the entire page width")]
			public bool HideLabel { get; set; }
		}

		#endregion

		#region Value Editor

        /// <summary>
        /// Creates a value editor instance
        /// </summary>
        /// <returns></returns>
		protected override IDataValueEditor CreateValueEditor()
		{
			return new ArchetypePropertyValueEditor(base.CreateValueEditor());
		}

        /// <summary>
        /// Class that represents the actual data editor. This is often done with a package manifest instead.
        /// </summary>
		internal class ArchetypePropertyValueEditor : DataValueEditor
		{
			protected JsonSerializerSettings _jsonSettings;

			public ArchetypePropertyValueEditor(IDataValueEditor wrapped)
				: base(wrapped.View,wrapped.Validators.ToArray())
			{
			}

            /// <summary>
            /// Converts the property value for use in the front-end cache
            /// </summary>
            /// <param name="property"></param>
            /// <param name="propertyType"></param>
            /// <param name="dataTypeService"></param>
            /// <returns></returns>
			public override string ConvertDbToString(PropertyType propertyType,object value, IDataTypeService dataTypeService)
			{
				if(value == null || value.ToString() == "")
					return string.Empty;

				var archetype = ArchetypeHelper.Instance.DeserializeJsonToArchetype(value.ToString(), propertyType.DataTypeId);

				foreach (var fieldset in archetype.Fieldsets)
				{
					foreach (var propDef in fieldset.Properties.Where(p => p.DataTypeGuid != null))
					{
						try
						{
							if(propDef == null || propDef.DataTypeGuid == null) continue;
							var dtd = ArchetypeHelper.Instance.GetDataTypeByGuid(Guid.Parse(propDef.DataTypeGuid));
							var propType = new PropertyType(dtd) {Alias = propDef.Alias};
							var prop = new Property(propType);
							prop.SetValue(propDef.Value);
							//var propEditor = PropertyEditorResolver.Current.GetByAlias(dtd.EditorAlias);
							var propEditor = Current.DataEditors.FirstOrDefault(de => de.Alias == dtd.EditorAlias);
							propDef.Value = propEditor.GetValueEditor().ConvertDbToString(propType,prop.GetValue(), dataTypeService);
						}
						catch (Exception ex)
						{
							Current.Logger.Error<ArchetypePropertyValueEditor>(ex.Message, ex);
						}
					}
				}

				return archetype.SerializeForPersistence();
			}

            /// <summary>
            /// A method used to format the database value to a value that can be used by the editor
            /// </summary>
            /// <param name="property"></param>
            /// <param name="propertyType"></param>
            /// <param name="dataTypeService"></param>
            /// <returns></returns>
            /// <remarks>
            /// The object returned will automatically be serialized into json notation. For most property editors
            /// the value returned is probably just a string but in some cases a json structure will be returned.
            /// </remarks>
			public override object ToEditor(Property property, IDataTypeService dataTypeService,string culture = null,string segment = null)
			{
				var value = property.GetValue();
				if (value == null || value.ToString() == "")
					return string.Empty;

				var archetype = ArchetypeHelper.Instance.DeserializeJsonToArchetype(value.ToString(), property.PropertyType.DataTypeId);

				foreach (var fieldset in archetype.Fieldsets)
				{
					foreach (var propDef in fieldset.Properties.Where(p => p.DataTypeGuid != null))
					{
						try
						{
							var dtd = ArchetypeHelper.Instance.GetDataTypeByGuid(Guid.Parse(propDef.DataTypeGuid));
							var propType = new PropertyType(dtd) {Alias = propDef.Alias};
							var prop = new Property(propType);
							prop.SetValue(propDef.Value);
							var propEditor = Current.DataEditors.FirstOrDefault(de => de.Alias == dtd.EditorAlias);
							propDef.Value = propEditor.GetValueEditor().ToEditor(prop, dataTypeService,culture,segment);
						}
						catch (Exception ex)
						{
							Current.Logger.Error<ArchetypePropertyValueEditor>(ex.Message, ex);
						}
					}
				}

				return archetype;
			}

            /// <summary>
            /// A method to deserialize the string value that has been saved in the content editor
            /// to an object to be stored in the database.
            /// </summary>
            /// <param name="editorValue"></param>
            /// <param name="currentValue">The current value that has been persisted to the database for this editor. This value may be usesful for
            /// how the value then get's deserialized again to be re-persisted. In most cases it will probably not be used.</param>
            /// <returns></returns>
            /// <remarks>
            /// By default this will attempt to automatically convert the string value to the value type supplied by ValueType.
            /// If overridden then the object returned must match the type supplied in the ValueType, otherwise persisting the
            /// value to the DB will fail when it tries to validate the value type.
            /// </remarks>
			public override object FromEditor(ContentPropertyData editorValue, object currentValue)
			{
				if(editorValue.Value == null || editorValue.Value.ToString() == "")
					return string.Empty;

				// attempt to deserialize the current property value as an Archetype
				var currentArchetype = currentValue != null ? ArchetypeHelper.Instance.DeserializeJsonToArchetype(currentValue.ToString(), editorValue.DataTypeConfiguration as System.Collections.Generic.Dictionary<string, object>) : null;
				var archetype = ArchetypeHelper.Instance.DeserializeJsonToArchetype(editorValue.Value.ToString(), editorValue.DataTypeConfiguration as System.Collections.Generic.Dictionary<string, object>);

				// get all files uploaded via the file manager (if any)
				var uploadedFiles = editorValue.Files;
				foreach (var fieldset in archetype.Fieldsets)
				{
					// make sure the publishing dates are in UTC
					fieldset.ReleaseDate = EnsureUtcDate(fieldset.ReleaseDate);
					fieldset.ExpireDate = EnsureUtcDate(fieldset.ExpireDate);

					// assign an id to the fieldset if it has none (e.g. newly created fieldset)
					fieldset.Id = fieldset.Id == Guid.Empty ? Guid.NewGuid() : fieldset.Id;
					// find the corresponding fieldset in the current Archetype value (if any)
					var currentFieldset = currentArchetype != null ? currentArchetype.Fieldsets.FirstOrDefault(f => f.Id == fieldset.Id) : null;
					foreach (var propDef in fieldset.Properties)
					{
						try
						{
							// find the corresponding property in the current Archetype value (if any)
							var currentProperty = currentFieldset != null ? currentFieldset.Properties.FirstOrDefault(p => p.Alias == propDef.Alias) : null;
							var dtd = ArchetypeHelper.Instance.GetDataTypeByGuid(Guid.Parse(propDef.DataTypeGuid));
							var preValues = Current.Services.DataTypeService.GetAll(dtd.Id);

							var additionalData = new Dictionary<string, object>();

							// figure out if we need to pass a files collection in the additional data to the property value editor
							if(uploadedFiles != null)
							{
								if(dtd.EditorAlias == Constants.PropertyEditorAlias)
								{
									// it's a nested Archetype - just pass all uploaded files to the value editor
									additionalData["files"] = uploadedFiles.ToList();
								}
								else if (propDef.EditorState != null && propDef.EditorState.FileNames != null && propDef.EditorState.FileNames.Any())
								{
									// pass the uploaded files that belongs to this property (if any) to the value editor
									var propertyFiles = propDef.EditorState.FileNames.Select(fileName =>
											uploadedFiles.FirstOrDefault(u => u.FileName != null &&
												// #384, #389 and #394 - look for "safe" file names (using the ToSafeFileName() extension)
												// - for backwards compatibility we'll look for the raw filename as well as the "safe" file name
												(u.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase) || u.FileName.Equals(fileName.ToSafeFileName(), StringComparison.OrdinalIgnoreCase))
											)
                                        ).Where(f => f != null).ToList();
									if(propertyFiles.Any())
									{
										additionalData["files"] = propertyFiles;
									}
								}
							}
							var propData = new ContentPropertyData(propDef.Value, preValues);
							var propEditor = Current.DataEditors.FirstOrDefault(de => de.Alias == dtd.EditorAlias);
							// make sure to send the current property value (if any) to the PE ValueEditor
							propDef.Value = propEditor.GetValueEditor().FromEditor(propData, currentProperty != null ? currentProperty.Value : null);
						}
						catch (Exception ex)
						{
							Current.Logger.Error<ArchetypePropertyValueEditor>(ex.Message, ex);
						}
					}
				}

				return archetype.SerializeForPersistence();
			}

            /// <summary>
            /// Gets the property editor.
            /// </summary>
            /// <param name="dtd">The DTD.</param>
            /// <returns></returns>
			internal virtual IDataEditor GetPropertyEditor(IDataType dtd)
			{
				if(dtd.Id != 0)
					return Current.DataEditors.FirstOrDefault(de => de.Alias == dtd.EditorAlias);

				return dtd.EditorAlias.Equals(Constants.PropertyEditorAlias)
					? new ArchetypePropertyEditor()
					: (IDataEditor) new TextboxPropertyEditor(Current.Logger);
			}

			/// <summary>
			/// Ensures that a datetime is in UTC
			/// </summary>
			/// <param name="dateTime">The datetime</param>
			/// <returns>The datetime in UTC (or null if it's not set)</returns>
			private DateTime? EnsureUtcDate(DateTime? dateTime)
			{
				if(dateTime.HasValue == false || dateTime.Value.Kind == DateTimeKind.Utc)
				{
					return dateTime;
				}
				return new DateTime(dateTime.Value.Ticks, DateTimeKind.Utc);
			}
		}

		#endregion
	}
}
