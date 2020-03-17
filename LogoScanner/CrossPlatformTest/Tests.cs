using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LogoScanner;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace CrossPlatformTest
{
    [TestFixture(Platform.Android)]
    [TestFixture(Platform.iOS)]
    public class Tests
    {
        private IApp app;
        private readonly Platform platform;

        public Tests(Platform platform)
        {
            this.platform = platform;
        }

        [SetUp]
        public void BeforeEachTest()
        {
            app = AppInitializer.StartApp(platform);
        }

        //get credentials file
        private JObject GetCredentials()
        {
            var assembly = typeof(LogoScanner.App).GetTypeInfo().Assembly;
            var credentialsFile = "LogoScanner.credentials.json";
            JObject line = new JObject();

            Stream stream = assembly.GetManifestResourceStream(credentialsFile);
            using (StreamReader reader = new StreamReader(stream))
            {
                line = JObject.Parse(reader.ReadToEnd()); // opens credentials file, reads it and parse JSON
            }
            
            return line;
        }

        // helper method to access Custom Vision, which returns HttpClient response
        private async Task<HttpResponseMessage> ConnectToCustomVisionForTesting(byte[] InputImage)
        {
            //new http client with key
            var client = new HttpClient();
            var credentials = GetCredentials();

            client.DefaultRequestHeaders.Add("Prediction-key", credentials["CustomVisionAPI"]["key"].ToString());

            //post image to url and check if received successfully
            HttpResponseMessage response;

            //get image
            byte[] image = InputImage;

            using (var content = new ByteArrayContent(image))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync(credentials["CustomVisionAPI"]["url"].ToString(), content);
            }
            return response;
        }

        // helper method to access ResDiary API, which returns HttpClient response
        private async Task<HttpResponseMessage> ConnectToResDiary()
        {
            //new http client with key
            var client = new HttpClient();
            var line = GetCredentials();

            string credentials = @"{""Username"" : """ + line["ResDiaryAPI"]["username"].ToString() + @""", ""Password"" : """ + line["ResDiaryAPI"]["password"].ToString() + @"""}"; // parse in username/password to json
            var content = new StringContent(credentials, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api.rdbranch.com/api/Jwt/v2/Authenticate", content); // get response from the api

            return response;
        }

        //test that app was successfully connected to custom vision api with post method
        [Test]
        public async Task IsAppConnectedToCustomVision()
        {
            // get image
            byte [] image = File.ReadAllBytes("../../logo-test.png");
            // call helper method to access API, which returns response from HttpClient
            HttpResponseMessage response = await ConnectToCustomVisionForTesting(image);

            Assert.AreEqual("OK", response.StatusCode.ToString(), "The response status code is " + response.StatusCode.ToString() + ", while expected OK.");
        }

        //test that app was not connected to custom vision api with post method
        [Test]
        public async Task IsAppNotConnectedToCustomVision()
        {
            // get image
            byte[] image = new byte[1987];
            // call helper method to access API, which returns response from HttpClient
            HttpResponseMessage response = await ConnectToCustomVisionForTesting(image);

            Assert.AreEqual("BadRequest", response.StatusCode.ToString(), "The response status code is " + response.StatusCode.ToString() + ", while expected BadRequest.");
        }

        // test if Custom Vision returns correct prediction for a picture
        [Test]
        public async Task IsCustomVisionReturningRightPrediction()
        {
            //get image
            byte[] image = File.ReadAllBytes("../../logo-test.png");
            // call helper method to access API, which returns response from HttpClient
            HttpResponseMessage response = await ConnectToCustomVisionForTesting(image);

            //get the name of the restaurant
            var resultJson = await response.Content.ReadAsStringAsync();
            var restaurantName = JsonConvert.DeserializeObject<PredictionResult>(resultJson);

            Assert.AreEqual("Union", restaurantName.ToString(), "The response status code is " + restaurantName.ToString() + ", while expected Union.");
        }

        //prediciton results to string return only one parameter with probability higher as 0.3
        [Test]
        public void ArePredicitionResultToStringReturningGoodElement()
        {
            var data1 = new PredictionResult
            {
                Predictions = new List<Prediction>{new Prediction{Probability=0.5, TagName="Restaurant1"},
                                           new Prediction{Probability=8.5, TagName="Restaurant2"},
                                           new Prediction{Probability=73.5, TagName="Restaurant3"},
                                           new Prediction{Probability=65.5, TagName="Restaurant4"}}
            };

            Assert.AreEqual("Restaurant3", data1.ToString(), "The Prediction Result is " + data1.ToString() + ", while expected Restaurant3.");
        }

        //prediciton results to string return only one parameter with probability higher as 0.3
        [Test]
        public void ArePredicitionResultToStringReturningEmptyString()
        {

            var data2 = new PredictionResult
            {
                Predictions = new List<Prediction>{new Prediction{Probability=0.2, TagName="Restaurant1"},
                                           new Prediction{Probability=0.01, TagName="Restaurant2"},
                                           new Prediction{Probability=0.1, TagName="Restaurant3"}}
            };

            Assert.AreEqual("", data2.ToString(), "The Prediction Result is " + data2.ToString() + ", while expected \"\".");
        }

        [Test]
        // test that there are only 3 default booking slot
        public void AreThere3DefaultBookingSlot()
        {
            var dateList = new List<string>();

            // take picture
            app.Tap("takePhotoButton");

            // go to the book page
            app.Tap(c => c.Property("text").Contains("Book"));

            for (int i = 0; i < 2; i++)
            {
                app.WaitForElement(x => x.Class("ConditionalFocusLayout"), timeout: TimeSpan.FromSeconds(120));
                if (!dateList.Contains(app.Query("BookTime")[0].Text))
                {
                    dateList.Add(app.Query("BookTime")[0].Text);
                }
                if (!dateList.Contains(app.Query("BookTime")[1].Text))
                {
                    dateList.Add(app.Query("BookTime")[1].Text);
                }
                app.ScrollDown();
            }

            // check if the default booking slot are 3
            Assert.AreEqual(3, dateList.Count);
        }

        [Test]
        // test that select the size of the booking slot is working
        public void AreSelectSlotForBookingWorking()
        {
            var dateList = new List<string>();
            Random rnd = new Random();
            int randomSlot = rnd.Next(1, 11);

            // take picture
            app.Tap("takePhotoButton");

            // go to the book page
            app.Tap(c => c.Property("text").Contains("Book"));

            // tap to select the amount of booking slot
            app.Tap(c => c.Property("text").Contains("3 SLOTS"));

            // random the amount of booking slot
            app.Tap(c => c.Property("text").Contains(randomSlot.ToString()));

            for (int i = 0; i < randomSlot; i++)
            {
                app.WaitForElement(x => x.Class("ConditionalFocusLayout"), timeout: TimeSpan.FromSeconds(120));
                if (!dateList.Contains(app.Query("BookTime")[0].Text))
                {
                    dateList.Add(app.Query("BookTime")[0].Text);
                }
                if (randomSlot > 1 && !dateList.Contains(app.Query("BookTime")[1].Text))
                {
                    dateList.Add(app.Query("BookTime")[1].Text);
                }
                app.ScrollDown();
            }

            // check if the booking slot display true
            Assert.AreEqual(randomSlot, dateList.Count);
        }

        // test that was successfully connected to resdiary api with post method
        [Test]
        public async Task IsAppConnectedToResDiary()
        {
            // call helper method to access API, which returns response from HttpClient
            HttpResponseMessage response = await ConnectToResDiary();

            Assert.AreEqual("OK", response.StatusCode.ToString(), "The response status code is " + response.StatusCode.ToString() + ", while expected OK.");
        }

        // check if the device supports camera
        [Test]
        public void IsDeviceSupportCamera()
        {
            // Camera contain 1 texture view & 3 app compat button; toggleFlashButton, cameraRectangle, takePhotoButton
            AppResult[] textureView = app.Query(x => x.Class("textureView"));
            AppResult[] cameraButton = app.Query(x => x.Class("AppCompatButton"));

            Assert.IsTrue(textureView.Any() & cameraButton.Length == 3, "Camera not exist");
        }
    }

}