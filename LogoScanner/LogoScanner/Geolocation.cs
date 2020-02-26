using Newtonsoft.Json.Linq;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace LogoScanner
{
    public class Geolocation
    {
        public static double myLatitude;
        public static double myLongitude;

        //check if returned string from Custom Vision has underscore _
        public static bool HasMoreOptions(String TagName)
        {
            return TagName.ToLower().Contains('_');
        }

        public static List<String> SplitMoreOptions(String TagName)
        {
            return new List<String>(TagName.Split('_'));
        }

        public static async Task<Location> GetMyLocation()
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Location);
                if (status != PermissionStatus.Granted)
                {

                    var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Location });
                    status = results[Permission.Location];
                }

                if (status == PermissionStatus.Granted)
                {
                    var location = await Xamarin.Essentials.Geolocation.GetLocationAsync(request);
                    if (location != null)
                    {
                        return new Location(location.Latitude, location.Longitude);
                    }
                }

                else if (status != PermissionStatus.Unknown)
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Location denied. Cannot continue, try again.", "OK");
                    return await GetMyLocation();
                }

                
            }
            catch (FeatureNotSupportedException)
            {
                // Handle not supported on device exception
                await App.Current.MainPage.DisplayAlert("Error", "Location feature is not supported on this device.", "OK");
            }
            catch (FeatureNotEnabledException)
            {
                // Handle not enabled on device exception
                await App.Current.MainPage.DisplayAlert("Error", "Location is not enabled on this device. Please turn on the location now.", "OK");
                return await GetMyLocation();
            }
            catch (PermissionException)
            {
                // Handle permission exception
                await App.Current.MainPage.DisplayAlert("Error", "Location feature is not permitted on this device.", "OK");
                return await GetMyLocation();
            }
            catch (Exception)
            {
                // Unable to get location
                await App.Current.MainPage.DisplayAlert("Error", "Unable to get location.", "OK");
            }
            return null;
        }

        public static async Task<string> GetCloserOptionAsync(String TagName)
        {
            //get my location
            var ListofOptions = SplitMoreOptions(TagName);
            Location myLocation = await GetMyLocation();
            //set highest distance
            double min = Double.MaxValue;
            String results = "";

            //search the closest to me
            foreach(String Name in ListofOptions)
            {
                Location NameLocation = await GetCoordinates(Name);
                var miles = Location.CalculateDistance(myLocation, NameLocation, DistanceUnits.Miles);
                if (miles < min)
                {
                    min = miles;
                    results = Name;
                }
            }
            return results;
        }

        public static async Task<Location> GetCoordinates(String micrositeName)
        {

            var request = await Requests.ConnectToResDiary(); // connect to resdiary api

            while (request.message.Equals("Unable to Connect to Internet"))
            {
                await App.Current.MainPage.DisplayAlert("Error", request.message, "OK"); // displays an error message to the user

                if (request.message == "Unable to Connect to Internet")
                {
                    request = await Requests.ConnectToResDiary();
                }
            }

            if (request.status.Equals("Success")) // if connection to api is successful
            {
                try
                {
                    JArray hasSummary = await Requests.APICallGet("https://api.rdbranch.com/api/ConsumerApi/v1/Restaurant/" + micrositeName + "/HasMicrositeSummary", request.message);
                    JObject result = (JObject)hasSummary.First;
                    if (result["Result"] != null)
                    {
                        JArray r = await Requests.APICallGet("https://api.rdbranch.com/api/ConsumerApi/v1/MicrositeSummaryDetails?micrositeNames=" + micrositeName + "&startDate=2019-11-19T10:53:39&endDate=2019-11-18T10:53:39&channelCodes=ONLINE&numberOfReviews=5", request.message);
                        JObject restaurant = (JObject)r.First;
                        return new Location(Convert.ToDouble(restaurant["Latitude"].ToString()), Convert.ToDouble(restaurant["Longitude"].ToString()));
                    }
                }
                catch (NullReferenceException)
                {
                    await App.Current.MainPage.DisplayAlert("Restaurant not found", "Please scan again.", "OK"); // Displays an error message to the user
                }
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Error", request.message, "OK"); // Displays an error message to the user
            }

            return null;
        }
    }
}
