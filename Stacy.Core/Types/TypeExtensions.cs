using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.CompilerServices;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;


namespace Stacy.Core.Types
{
	public static class TypeExtensions
	{
		/// <summary>
		/// Determine whether a type is simple (String, Decimal, DateTime, etc) 
		/// or complex (i.e. custom class with public properties and methods).
		/// </summary>
		/// <see cref="http://stackoverflow.com/questions/2442534/how-to-test-if-type-is-primitive"/>
		public static bool IsSimpleType(this Type type)
		{
			return
					type.IsValueType ||
					type.IsPrimitive ||
					new Type[] { 
						typeof(String),
						typeof(Decimal),
						typeof(DateTime),
						typeof(DateTimeOffset),
						typeof(TimeSpan),
						typeof(Guid)
					}.Contains(type) ||
					Convert.GetTypeCode(type) != TypeCode.Object;
		}

		/// <summary>
		/// Determine whether a type is a valid email
		/// </summary>
		/// <param name="string">The string to check.</param>
		/// <returns>boolean</returns>
		public static bool IsValidEmail(this string email)
		{
			try
			{
				var addr = new MailAddress(email);
				return addr.Address == email;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Converts the Fluent Validation result to the type the both mvc and ef expect
		/// </summary>
		/// <param name="validationResult">The validation result.</param>
		/// <returns></returns>
		/*public static IEnumerable<ValidationResult> ToValidationResult(
			this FluentValidation.Results.ValidationResult validationResult)
		{
			var results = validationResult.Errors.Select(item => new ValidationResult(item.ErrorMessage, new List<string> { item.PropertyName }));
			return results;
		}*/

		/// <summary>
		/// Converts an integer to the Generic's type
		/// </summary>
		/// <param name="e">The value to convert to enum.</param>
		/// <returns></returns>
		public static T ToEnum<T>(this int e) where T: struct, IConvertible
		{
			if(!typeof(T).IsEnum)
				throw new ArgumentException("T must be an enumerated type");

			return (T)Enum.ToObject(typeof(T), e);
		}

        /// <summary>
        /// Test if method info is marked async
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static bool IsAsyncMethod(MethodInfo method)
        {
            var attType = typeof(AsyncStateMachineAttribute);

            // Obtain the custom attribute for the method. 
            // The value returned contains the StateMachineType property. 
            // Null is returned if the attribute isn't present for the method. 
            var attrib = (AsyncStateMachineAttribute)method?.GetCustomAttribute(attType);

            return (attrib != null);
        }
    }
}
