using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Stacy.Core.Data
{
    public static class DbContextExtensions
    {
        public static T NewInstance<T>(this T context)
        {
            var type = context.GetType();

            var constructor = type.GetConstructors()
                .OrderBy(c => c.GetParameters().Length)
                .FirstOrDefault();

            if (constructor == null)
                throw new Exception("Error creating new DbContext instance: Context type has no constructors");

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var parameters = constructor.GetParameters();
            var parameterValues = new List<object>();

            foreach (var parameter in parameters)
            {
                var field = fields.FirstOrDefault(f => 
                    string.Equals(f.Name.Replace("_", ""), parameter.Name, StringComparison.InvariantCultureIgnoreCase)
                    && f.FieldType.Name == parameter.ParameterType.Name);
                if (field == null)
                    throw new Exception("Error creating new DbContext instance: constructor parameters not available in original instance");

                parameterValues.Add(field.GetValue(context));
            }

            return (T)Activator.CreateInstance(type, parameterValues.ToArray());

        }
    }
}

