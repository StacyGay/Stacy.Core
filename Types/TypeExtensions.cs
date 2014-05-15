using System;
using System.Collections.Generic;
using System.Linq;

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
		/// Converts the Fluent Validation result to the type the both mvc and ef expect
		/// </summary>
		/// <param name="validationResult">The validation result.</param>
		/// <returns></returns>
		public static IEnumerable<ValidationResult> ToValidationResult(
			this FluentValidation.Results.ValidationResult validationResult)
		{
			var results = validationResult.Errors.Select(item => new ValidationResult(item.ErrorMessage, new List<string> { item.PropertyName }));
			return results;
		}
	}
}
