using System;

namespace Stacy.Core.Connection
{
    public class CommunicationLog
    {
        public int LogId { get; set; }
        public string MessageId { get; set; } = Guid.NewGuid().ToString();
        public string Operation { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public DateTime RequestWhen { get; set; } = DateTime.Now;
        public DateTime ResponseWhen { get; set; } = DateTime.Now;
        public string StackTrace { get; set; }
        public double ElapsedSeconds => CallTime.TotalSeconds;
        public double ElapsedMilliseconds => CallTime.TotalMilliseconds;
        public TimeSpan CallTime => ResponseWhen - RequestWhen;
        public DateTime AddedOn { get { return DateTime.Now; } }
        public string StatusCode { get; set; }

        public bool Retry { get; set; } = false;
    }
}
