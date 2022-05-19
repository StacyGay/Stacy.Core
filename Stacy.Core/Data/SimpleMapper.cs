using Newtonsoft.Json;
using System;
using System.Linq;

namespace Stacy.Core.Data
{
    /// <summary>
    /// Object mapper tool that produces a shallow copy of one type into another
    /// Note: internal objects are not cloned and retain the original reference of the source
    /// </summary>
    public interface ISimpleMapper
    {
        TDestination Map<TSource, TDestination>(TSource source) where TDestination : new();
        //void Map<TSource, TDestination>(TSource source, TDestination destination) where TDestination : new();
        void Map(object source, object destination);

        /// <summary>
        /// Uses serialization to copy an object to a similar destination
        /// </summary>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <returns>Null on failure, New TDestination on success</returns>
        TDestination LikeCopy<TDestination>(object source) where TDestination : new();
    }

    public class SimpleMapper : ISimpleMapper
    {
        public void Map(object source, object destination)
        {
            if (source == null)
                return;

            var sourceType = source.GetType();
            var destType = destination.GetType();
            var sourceProps = sourceType.GetProperties();
            var sourceFields = sourceType.GetFields();

#if DEBUG
            if (!sourceProps.Any())
                System.Diagnostics.Debug.WriteLine("Simple mapper found no fields to map: " + sourceType.Name + " to " + destType.Name);
#endif

            try
            {
                foreach (var sourceProp in sourceProps)
                {
                    try
                    {
                        var destProp = destType.GetProperty(sourceProp.Name);
                        if (destProp == null || destProp.GetSetMethod() == null) continue;
                        destProp.SetValue(destination, sourceProp.GetValue(source));
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Simple mapper - Error mapping object property: ", ex.Message);
                    }
                }

                foreach (var sourceField in sourceFields)
                {
                    try
                    {
                        var destField = destType.GetField(sourceField.Name);
                        if (destField == null) continue;
                        destField.SetValue(destination, sourceField.GetValue(source));
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Simple mapper - Error mapping object property: ", ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error mapping object: ", ex.Message);
            }
        }

        public TDestination Map<TSource, TDestination>(TSource source) where TDestination : new()
        {
            var result = new TDestination();
            Map(source, result);
            return result;
        }

        /// <summary>
        /// Uses serialization to copy an object to a similar destination
        /// </summary>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <returns>Null on failure, New TDestination on success</returns>
        public TDestination LikeCopy<TDestination>(object source) where TDestination : new()
        {
            try
            {
                var json = source.ToJson();
                return JsonConvert.DeserializeObject<TDestination>(json, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects
                });
            }
            catch(Exception ex)
            {
               System.Diagnostics.Debug.WriteLine("Error copying object: ", ex.Message);
            }

            return default(TDestination);
        }
    }
}
