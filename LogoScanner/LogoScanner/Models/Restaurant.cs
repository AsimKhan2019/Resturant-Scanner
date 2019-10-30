using System;
using System.Collections.Generic;
using System.Text;

namespace LogoScanner.Models
{
    public class Restaurant
    {
        public string Name { get; set; }
        public List<string> Cuisines { get; set; }

        public int Rating { get; set; }

        public int NoRatings { get; set; }

        public int PricePoint { get; set; }

        public bool FreeTables { get; set; }

        public List<String> AvailableSlots { get; set; }
    }
}
