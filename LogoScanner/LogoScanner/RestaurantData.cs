using System;
using System.Collections.Generic;
using System.Text;

namespace LogoScanner
{
    public class RestaurantData
    {
        public string Name { get; set; }
        public string FullAddress { get; set; }
        public string LogoUrl { get; set; }
        public int NumberOfReviews { get; set; }
        public double AverageReviewScore { get; set; }
        public object AvailableTimeSlots { get; set; }
        public object CuisineTypes { get; set; }
        public int PricePoint { get; set; }
    }
}
