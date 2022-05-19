using System;
using Stacy.Core.Date;

namespace Stacy.Core.Data
{
    public class StringTemplateParserVariables
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime ArrivalDate { get; set; }
        public DateTime DepartureDate { get; set; }

        public int NumberOfNights
        {
            get
            {
                try
                {
                    return new DateRange(ArrivalDate, DepartureDate).NumberOfNights;
                }
                catch (Exception ex)
                {
                    return 0;
                }
            }
        }

        public int Adults { get; set; }
        public int Children { get; set; }
        public int Guests => Adults + Children;
        public string UnitTypeName { get; set; }
        public int UnitTypeId { get; set; }
        public int PackageId { get; set; }
        public string PackageName { get; set; }
        public string ReservationsPhone { get; set; }
        public int MinimumNights { get; set; }
    }

    public static class StringTemplateParser
    {
        public static string ParseTemplate(this string thisString, StringTemplateParserVariables variables)
        {
            return thisString
                .Replace("{{FirstName}}", variables.FirstName)
                .Replace("{{LastName}}", variables.LastName)
                .Replace("{{ArrivalDate}}", variables.ArrivalDate.ToString("d"))
                .Replace("|arrivalData|", variables.ArrivalDate.DayOfWeek.ToString())
                .Replace("{{DepartureDate}}", variables.DepartureDate.ToString("d"))
                .Replace("|departureDate|", variables.DepartureDate.DayOfWeek.ToString())
                .Replace("{{NumberOfNights}}", variables.NumberOfNights.ToString())
                .Replace("{{Adults}}", variables.Adults.ToString())
                .Replace("{{Children}}", variables.Children.ToString())
                .Replace("{{Guests}}", variables.Guests.ToString())
                .Replace("{{GuestCount}}", variables.Guests.ToString())
                .Replace("{{UnitTypeName}}", variables.UnitTypeName)
                .Replace("{{UnitTypeId}}", variables.UnitTypeId.ToString())
                .Replace("{{PackageId}}", variables.PackageId.ToString())
                .Replace("{{PackageName}}", variables.PackageName)
                .Replace("{{ReservationsPhone}}", variables.ReservationsPhone)
                .Replace("|phone|", variables.ReservationsPhone)
                .Replace("{{MinNights}}", variables.MinimumNights.ToString())
                .Replace("|minNights|", variables.MinimumNights.ToString());
        }
    }
}
