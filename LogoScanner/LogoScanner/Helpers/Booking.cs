using System;
using System.Globalization;
using Xamarin.Essentials;

namespace LogoScanner
{
    public class Booking
    {

        public static void Makebooking(string microsite, string date, string time, int partysize)
        {
            //Passed in as "Date: 29/01/2020"
            //Passed in as "Time: 11:45:00"

            string bookingdate = string.Format(CultureInfo.CurrentCulture,"{0}-{1}-{2}", date.Substring(6, 4), date.Substring(3, 2), date.Substring(0, 2));

            string booking = "https://book.rdbranch.com/Restaurant/" + microsite +
                        "/Book/Customer?bookingDateTime=" + bookingdate +
                        "T" + time.Substring(0, 2) +
                        "%3A" + time.Substring(3, 2) +
                        "%3A00&covers=" + partysize.ToString(CultureInfo.CurrentCulture);

            Launcher.OpenAsync(booking);
        }
    }
}