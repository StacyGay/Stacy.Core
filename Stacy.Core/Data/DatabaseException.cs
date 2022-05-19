using System;

namespace Stacy.Core.Data
{
    [Serializable]
    public class DatabaseException : Exception
    {
        public string Query { get; set; }
        public object Parameters { get; set; }

        public DatabaseException()
        {
            
        }

        public DatabaseException(string message) : base(message)
        {
            
        }

        public DatabaseException(string message, Exception innerException, string query = "", object parameters = null) : base(message, innerException)
        {
            Query = query;
            Parameters = parameters;
        }
    }
}
