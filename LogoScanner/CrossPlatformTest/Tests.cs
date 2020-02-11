using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using LogoScanner;
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
        private Platform platform;

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
        private JObject getCredentials()
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

        //test that app was successfully connected to custom vision api with post method
        [Test]
        public async Task IsAppConnectedToCustomVision()
        {
            //new http client with key
            var client = new HttpClient();
            var credentials = getCredentials();

            client.DefaultRequestHeaders.Add("Prediction-key", credentials["CustomVisionAPI"]["key"].ToString());

            //post image to url and check if received successfully
            HttpResponseMessage response;

            //get image
            byte[] image = System.IO.File.ReadAllBytes("./CrossPlatformTest/logo-test.png");

            using (var content = new ByteArrayContent(image))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync(credentials["CustomVisionAPI"]["url"].ToString(), content);
            }
            Assert.AreEqual("OK", response.StatusCode.ToString(), "The response status code is " + response.StatusCode.ToString() + ", while expected OK.");
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
            var data2 = new PredictionResult
            {
                Predictions = new List<Prediction>{new Prediction{Probability=0.2, TagName="Restaurant1"},
                                           new Prediction{Probability=0.01, TagName="Restaurant2"},
                                           new Prediction{Probability=0.1, TagName="Restaurant3"}}
            };

            Assert.AreEqual("Restaurant3", data1.ToString(), "The Prediction Result is " + data1.ToString() + ", while expected Restaurant3.");
            Assert.AreEqual("", data2.ToString(), "The Prediction Result is " + data2.ToString() + ", while expected \"\".");
        }
    }
}