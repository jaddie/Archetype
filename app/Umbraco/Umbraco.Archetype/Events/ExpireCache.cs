using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;

namespace Archetype.Events
{
    public class ExpireCacheComposer : ComponentComposer<ExpireCacheComponent>
    { }
    public class ExpireCacheComponent : IComponent
    {
        public void Initialize()
        {
            DataTypeService.Saved += ExpirePreValueCache;
        }

        public void Terminate()
        {
        }


        /// <summary>
        /// Expires the pre value cache when a datatype is saved.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Umbraco.Core.Events.SaveEventArgs{IDataTypeDefinition}"/> instance containing the event data.</param>
        void ExpirePreValueCache(IDataTypeService sender, Umbraco.Core.Events.SaveEventArgs<IDataType> e)
        {
            foreach (var dataType in e.SavedEntities)
            {
                Current.AppCaches.RuntimeCache.ClearByKey(Constants.CacheKey_PreValueFromDataTypeId + dataType.Id);
                Current.AppCaches.RuntimeCache.ClearByKey(Constants.CacheKey_DataTypeByGuid + dataType.Key);
            }
        }
    }
}
