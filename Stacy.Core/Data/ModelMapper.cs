using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Stacy.Core.Data
{
    public static class ModelMapperExtensions
    {
        /*public static void MapChildren<TParent, TChild>(this List<TParent> parents, IEnumerable<TChild> children, Func<TParent, TChild, bool> mapOn)
        {
            MapChildren(parents.AsEnumerable(), children, mapOn);
        }*/

        public static void MapChildren<TParent, TChild>(this IEnumerable<TParent> parents, IEnumerable<TChild> children, Func<TParent, TChild, bool> mapOn)
        {

            var childrenList = children.ToSafeList();
            var parentList = parents.ToSafeList();

            if (parentList == null || !parentList.Any() || childrenList == null || !childrenList.Any())
                return;

            var parentType = typeof (TParent);
            var childType = typeof (TChild);

            var parentProperty = parentType
                .GetProperties()
                .FirstOrDefault(p => 
                    (typeof (IEnumerable).IsAssignableFrom(p.PropertyType)
                        && p.PropertyType.GetGenericArguments().FirstOrDefault() == childType)
                    || p.PropertyType == childType);

            var childProperty = childType
                .GetProperties()
                .FirstOrDefault(p => 
                    (typeof(IEnumerable).IsAssignableFrom(p.PropertyType)
                        && p.PropertyType.GetGenericArguments().FirstOrDefault() == parentType)
                    || p.PropertyType == parentType);


            foreach (var parent in parentList)
                foreach (var child in childrenList.Where(c => mapOn(parent, c)))
                    SetMapValues(parent, parentProperty, child, childProperty);
        }

        private static void SetMapValues<TParent, TChild>(TParent parent, PropertyInfo parentProperty, TChild child, PropertyInfo childProperty)
        {
            MapOneWay(parent, parentProperty, child);
            MapOneWay(child, childProperty, parent);
        }

        private static void MapOneWay<TParent, TChild>(TParent parent, PropertyInfo parentProperty, TChild child)
        {
            if (!typeof(IEnumerable).IsAssignableFrom(parentProperty.PropertyType))
            {
                parentProperty.SetValue(parent, child);
                return;
            }

            var parentPropertyValue = parentProperty.GetValue(parent);
            var list = ((IEnumerable<TChild>)parentPropertyValue).ToSafeList();
            list.Add(child);
            parentProperty.SetValue(parent, list);
        }

        public static void MapChildren<TParent, TChild>(this TParent parent, IEnumerable<TChild> children,Func<TParent, TChild, bool> mapOn)
        {
            new List<TParent> { parent }.MapChildren(children, mapOn);
        }
    }
}
