using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using LogoScanner.Themes;
using Newtonsoft.Json.Linq;

namespace LogoScanner.Helpers
{
    public class Promotions
    {
        public static void GetAvailablePromotions(JObject r, int slotNumber)
        {
            RestaurantPage.availableTimes.Clear();
            int capacity = 0;

            foreach (var day in r["AvailableDates"])
            {
                if (capacity <= slotNumber)
                {
                    Dictionary<string, string> areas = new Dictionary<string, string>();

                    foreach (var area in day["Areas"])
                    {
                        areas.Add(area["Id"].ToString(), area["Name"].ToString());
                    }
                    foreach (var time in day["AvailableTimes"])
                    {
                        if (capacity == slotNumber)
                        {
                            break;
                        }
                        else
                        {
                            StringBuilder availableAreas = new StringBuilder();
                            StringBuilder timeSlot = new StringBuilder();

                            timeSlot.Append(time["TimeSlot"].ToString());

                            foreach (var availArea in time["AvailableAreaIds"])
                            {
                                availableAreas.Append(areas[availArea.ToString()]);
                                availableAreas.Append(" • ");
                            }
                            availableAreas.Remove(availableAreas.Length - 2, 2);

                            DateTime date = Convert.ToDateTime(day["Date"].ToString());

                            AvailableTime at = new AvailableTime
                            {
                                Date = date.Date.ToString("dd/MM/yyyy"),
                                Time = timeSlot.ToString().Substring(0, 5),
                                RestaurantAreas = availableAreas.ToString().Replace("\t", ""),
                                StringDate = date.Day + " " + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(date.Month) + " " + date.Year
                            };

                            GetValidPromotions(at);
                            RestaurantPage.availableTimes.Add(at);

                            capacity += 1;
                        }
                    }
                }
            }
        }

        public static string[] GetPromotionIDs(JObject json)
        {
            string[] value;
            if (json["AvailablePromotions"].Type == JTokenType.Null || string.IsNullOrEmpty(json["AvailablePromotions"].ToString()))
            {
                value = new string[0];
                value[0] = "No Promotions Currently Available";
                return value;
            }
            else
            {
                JToken[] promotionsIDs = json["AvailablePromotions"].ToArray();

                int i = 0;
                value = new string[promotionsIDs.Length];

                foreach (string id in promotionsIDs)
                {
                    value[i] = id;
                    i++;
                }
                return value;
            }
        }

        public static AvailableTime GetValidPromotions(AvailableTime current)
        {
            StringBuilder currentTime = new StringBuilder();
            StringBuilder allPromotions = new StringBuilder();

            IFormatProvider provider = CultureInfo.InvariantCulture;

            currentTime.Append(current.Date);
            currentTime.Append(" ");
            currentTime.Append(current.Time);
            currentTime.Append(":00");

            DateTime dateofBooking = DateTime.ParseExact(currentTime.ToString(), "dd/MM/yyyy HH:mm:ss", provider);
            foreach (Promotion p in RestaurantPage.promotions)
            {
                if (!string.IsNullOrEmpty(p.StartDate) && !string.IsNullOrEmpty(p.StartTime) && !string.IsNullOrEmpty(p.EndDate) && !string.IsNullOrEmpty(p.EndTime))
                {
                    StringBuilder start = new StringBuilder();
                    StringBuilder end = new StringBuilder();

                    start.Append(p.StartDate);
                    start.Append(" ");
                    start.Append(p.StartTime);

                    end.Append(p.EndDate);
                    end.Append(" ");
                    end.Append(p.EndTime);

                    DateTime startPromo = DateTime.ParseExact(start.ToString(), "dd/MM/yyyy HH:mm:ss", provider);
                    DateTime endPromo = DateTime.ParseExact(end.ToString(), "dd/MM/yyyy HH:mm:ss", provider);

                    int res1 = DateTime.Compare(dateofBooking, startPromo);  //Should return 1 or 0 - as DateofBooking should be >= startPromo
                    int res2 = DateTime.Compare(dateofBooking, endPromo);    //Should return -1 or 0 - as DateofBooking should be =< end Promo

                    if (res1 >= 0 && res2 <= 0)
                    {
                        allPromotions.Append(p.Name + " • " + p.Description + "\n");
                    }
                }
            }

            if (allPromotions.Length > 0)
                current.Promotions = allPromotions.ToString();
            else
                current.Promotions = "No Promotions Available";

            return current;
        }
    }
}