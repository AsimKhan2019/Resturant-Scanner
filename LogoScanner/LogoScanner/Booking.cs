using System;
using Xamarin.Essentials;

namespace Logoscanner
{
    public class Booking
    {
        public Booking()
        {
        }

        public void Makebooking(string microsite, string date, string time)
        {
            string bookingdate = "";
            string bookingtime = "";
            string booking = "https://book.rdbranch.com/Restaurant/" + microsite +
                        "/Book/Customer?bookingDateTime=" + bookingdate +
                        "2020-01-29" +
                        "T" + bookingtime.Substring(0, 2) +
                        "%3A" + bookingtime.Substring(2, 2) +
                        "%3A00&covers=" + "3";

            Launcher.OpenAsync(booking);
        }
    }
}