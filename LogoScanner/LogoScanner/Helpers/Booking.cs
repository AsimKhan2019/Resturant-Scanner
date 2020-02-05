using System;
using Xamarin.Essentials;

namespace Logoscanner
{
    public class Booking
    {
        public Booking()
        {
        }

        public static void Makebooking(string microsite, string date, string time)
        {
            //Passed in as "Date: 29/01/2020"
            //Passed in as "Time: 11:45:00"

            string bookingdate = string.Format("{0}-{1}-{2}", date.Substring(6, 4), date.Substring(3, 2), date.Substring(0, 2));

            string booking = "https://book.rdbranch.com/Restaurant/" + microsite +
                        "/Book/Customer?bookingDateTime=" + bookingdate +
                        "T" + time.Substring(0, 2) +
                        "%3A" + time.Substring(3, 2) +
                        "%3A00&covers=" + "3";

            Launcher.OpenAsync(booking);
        }
    }
}