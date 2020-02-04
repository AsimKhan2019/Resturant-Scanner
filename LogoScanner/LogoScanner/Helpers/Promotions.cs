using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace LogoScanner.Helpers
{
    public class Promotions
    {
        public static async void GetAvailablePromotions(string url, string token)
        {
            var dateStart = DateTime.Now;
            var dateStartStr = dateStart.ToString("yyyy-MM-ddTHH:mm:ss");

            var dateEnd = DateTime.Now.AddDays(7.00);
            var dateEndStr = dateEnd.ToString("yyyy-MM-ddTHH:mm:ss");

            JObject r = await Requests.APICallPost(url, token, dateStartStr, dateEndStr, 3);

            var capacity = 0;
            RestaurantPage.availableTimes.Clear();

            if (r != null)
            {
                foreach (var day in r["AvailableDates"])
                {
                    if (capacity <= 3)
                    {
                        Dictionary<string, string> areas = new Dictionary<string, string>();

                        foreach (var area in day["Areas"])
                        {
                            areas.Add(area["Id"].ToString(), area["Name"].ToString());
                        }

                        foreach (var time in day["AvailableTimes"])
                        {
                            if (capacity == 3)
                            {
                                break;
                            }
                            else
                            {
                                StringBuilder availableAreas = new StringBuilder();
                                StringBuilder timeSlot = new StringBuilder();

                                timeSlot.Append(time["TimeSlot"].ToString());

                                foreach (var availarea in time["AvailableAreaIds"])
                                {
                                    availableAreas.Append(areas[availarea.ToString()]);
                                    availableAreas.Append(", ");
                                }
                                availableAreas.Remove(availableAreas.Length - 2, 2);

                                string available = "Book to avoid disappointment";
                                string colour = "Gray";

                                if (capacity == 0)
                                {
                                    available = "AVAILABLE NOW";
                                    colour = "LimeGreen";
                                }

                                AvailableTime at = new AvailableTime
                                {
                                    Date = day["Date"].ToString().Substring(0, 10),
                                    Time = timeSlot.ToString().Substring(0,5),
                                    RestaurantAreas = availableAreas.ToString().Replace("\t",""),
                                    Available = available,
                                    Colour = colour
                                };

                                GetValidPromotions(at);
                                RestaurantPage.availableTimes.Add(at);

                                capacity += 1;
                            }
                        }
                    }
                }
            }
        }

        public static string[] GetPromotionIDs(JObject json)
        {
            string[] value;
            RestaurantPage.promotions.Clear();

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

            currentTime.Append(current.Date);
            currentTime.Append(" ");
            currentTime.Append(current.Time);
            currentTime.Append(":00");

            DateTime dateofBooking = DateTime.ParseExact(currentTime.ToString(), "dd/MM/yyyy HH:mm:ss", null);
            foreach (Promotion p in RestaurantPage.promotions)
            {
                StringBuilder start = new StringBuilder();
                StringBuilder end = new StringBuilder();

                start.Append(p.StartDate);
                start.Append(" ");
                start.Append(p.StartTime);

                end.Append(p.EndDate);
                end.Append(" ");
                end.Append(p.EndTime);

                DateTime startPromo = DateTime.ParseExact(start.ToString(), "dd/MM/yyyy HH:mm:ss", null);
                DateTime endPromo = DateTime.ParseExact(end.ToString(), "dd/MM/yyyy HH:mm:ss", null);

                int res1 = DateTime.Compare(dateofBooking, startPromo);  //Should return 1 or 0 - as DateofBooking should be >= startPromo
                int res2 = DateTime.Compare(dateofBooking, endPromo);    //Should return -1 or 0 - as DateofBooking should be =< end Promo

                if (res1 >= 0 && res2 <= 0)
                {
                    allPromotions.Append(p.Name);
                    allPromotions.Append("\n");
                    allPromotions.Append(p.Description);
                    allPromotions.Append("\n\n");
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
