using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace DroneWeatherAnalyzer.Controllers
{
    public class DroneWeatherAnalyzerController : Controller
    {

        private const string _ApiKey = "c5dc795eb4a7c047134466ea058da319";

        /// <summary>
        /// On the first start of the program it takes current Country and then with public API
        /// retrieve the country Capital city name and paste it in the form
        /// </summary>
        /// <returns> View </returns>
        // GET: QuotesAPI
        [HttpGet]
        public ActionResult DroneWeatherAnalyzer()
        {
           
            
            Dictionary<string, double> dronesWindResist = DictionaryInitializer();
            List<SelectListItem> droneList = dronesWindResist.Select(d => new SelectListItem
            {
                Text = d.Key
            }).ToList();
            ViewBag.Drones = droneList;

            //this retrieve current country name
            string countryName = RegionInfo.CurrentRegion.DisplayName;
            //and then with free api gets the Capital of the current country
            string capitalName = FetchCapitalOfCountry(countryName);

            List<string> geolocation = FetchGeolocation(capitalName);
            if (!geolocation.Any())
            {
                ViewData["alert"] =
                    "Please be sure that the name of the city that you insert is valid and try again!";
                ViewData["alertLevel"] = "caution";
                return View();
            };

            

            var url = $"https://api.openweathermap.org/data/2.5/weather?lat={geolocation[0]}&lon={geolocation[1]}&units=metric&APPID={_ApiKey}";
            var client = new WebClient();

            var body = client.DownloadString(url);

            JObject data = JObject.Parse(body);
            int mainTempInCelsius = (int)(data["main"]["temp"]);
            double visibilityInKm = (double)data["visibility"] / 1000;
            double windSpeed = (double)data["wind"]["speed"];


            ViewData["temp"] = mainTempInCelsius;
            ViewData["city"] = data["name"];
            ViewData["country"] = data["sys"]["country"];
            ViewData["weather"] = data["weather"][0]["main"];
            ViewData["humidity"] = data["main"]["humidity"];
            ViewData["wind"] = windSpeed;
            ViewData["visibility"] = visibilityInKm;
            ViewData["atmPressure"] = data["main"]["pressure"];


            return View();
        }

        // POST: QuotesAPI
        [HttpPost]
        public ActionResult DroneWeatherAnalyzer(string city, string selectedDroneModel)
        {

            Dictionary<string, double> dronesWindResist = DictionaryInitializer();
            List<SelectListItem> droneList = dronesWindResist.Select(d => new SelectListItem
            {
                Text = d.Key
            }).ToList();
            ViewBag.Drones = droneList;

            List<string> geolocation = FetchGeolocation(city);
            if (!geolocation.Any())
            {
                ViewData["alert"] =
                    "Please be sure that the name of the city that you insert is valid and try again!";
                ViewData["alertCity"] = "caution";
                return View();
            };

 
            var url = $"https://api.openweathermap.org/data/2.5/weather?lat={geolocation[0]}&lon={geolocation[1]}&units=metric&APPID={_ApiKey}";
            var client = new WebClient();

            var body = client.DownloadString(url);

            JObject data = JObject.Parse(body);
            int mainTempInCelsius = (int)(data["main"]["temp"]);
            double visibilityInKm = (double)data["visibility"] / 1000;
            double windSpeed = (double)data["wind"]["speed"];
            string weather = data["weather"][0]["main"].ToString();


            ViewData["temp"] = mainTempInCelsius;
            ViewData["city"] = data["name"];
            ViewData["country"] = data["sys"]["country"];
            ViewData["weather"] = weather;
            ViewData["humidity"] = data["main"]["humidity"];
            ViewData["wind"] = windSpeed;
            ViewData["visibility"] = visibilityInKm;
            ViewData["atmPressure"] = data["main"]["pressure"];

            // var droneWindResist = dronesWindResist[droneModel];
            double droneWindResist = dronesWindResist.Where(x => x.Key == selectedDroneModel).FirstOrDefault().Value;

            if (droneWindResist <= windSpeed )
            {
                ViewData["alert"] = $"Fly with caution! Your drone has wind resist of" +
                    $" {droneWindResist} m/s but the wind in the region is {windSpeed} m/s";
                ViewData["alertWind"] = "caution";
            }
            else
            {
                ViewData["alert"] = "Flying your drone is completely safe!";
                ViewData["alertWind"] = "missing";
            }

            if (weather.Contains("Rain") || weather.Contains("Snow"))
            {
                ViewData["alertRain"] = $"Flying your drone is not recommended because of the weather!";
                ViewData["alertWeather"] = "attention";
            }



            return View();
        }


        /// <summary>
        /// This will be used for post method which will capture input of the user(name of the city)
        /// and translate it to geolocation cordinates
        /// </summary>
        /// <param name="city"></param>
        /// <returns>  List<string> with 2 params - lat and lon </returns>
        private List<string> FetchGeolocation(string city)
        {
            var url = $"http://api.openweathermap.org/geo/1.0/direct?q={city}&limit=1&appid={_ApiKey}";
            var client = new WebClient();
            var body = client.DownloadString(url);
            JArray data = JArray.Parse(body);
            List<string> geolocations = new List<string>();

            if (data.HasValues)
            {
                var info1 = (Math.Round((double)data[0]["lat"], 2));
                var info2 = (Math.Round((double)data[0]["lon"], 2));
                geolocations.Add(info1.ToString());
                geolocations.Add(info2.ToString());
            }




            return geolocations;
        }



        /// <summary>
        /// Use to save and initilize a Dictionary which have { Model of Drone , windResist }
        /// </summary>
        /// <returns> Dictionary<string, double> </returns>
        private Dictionary<string, double> DictionaryInitializer()
        {
            return
           new Dictionary<string, double>() {
            { "DJI Mini", 15 },
            { "DJI Mini SE", 8},
            { "DJI Mini 2", 8.5},
            { "DJI Mini 2 SE", 10.7},
            { "DJI Mini 3", 10},
            { "DJI Mini 3 Pro", 10.7},

            { "DJI Avata", 10.7},

            { "DJI Phantom 3", 10},
            { "DJI Phantom 4", 10},

            { "DJI Mavic Air", 10},
            { "DJI Air 2", 10.5},
            { "DJI Air 2S", 10},

            { "DJI Mavic Pro", 10},
            { "DJI Mavic 2", 10},
            { "DJI Mavic 3", 15},

        };
        }

        /// <summary>
        /// This method is using api that retrieve info from
        /// name of the country and I use it to get the capital city of country
        /// </summary>
        /// <param name="country"></param>
        /// <returns> string capitalName </returns>
        private string FetchCapitalOfCountry(string country)
        {
            //free api on web 
            var url = $"https://restcountries.com/v3.1/translation/{country.ToLower()}";
            var client = new WebClient();
            var body = client.DownloadString(url);
            JArray data = JArray.Parse(body);
            string capitalCityName = data[0]["capital"][0].ToString();
            //to be sure I trim the result from errors after
            capitalCityName.Trim();
            return capitalCityName;

        }
    }
}