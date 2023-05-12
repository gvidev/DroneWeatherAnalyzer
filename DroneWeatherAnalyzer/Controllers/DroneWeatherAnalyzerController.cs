using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace DroneWeatherAnalyzer.Controllers
{
    public class DroneWeatherAnalyzerController : Controller
    {
        
        private const string _ApiKey = "c5dc795eb4a7c047134466ea058da319";

        // GET: QuotesAPI
        public ActionResult DroneWeatherAnalyzer()
        {
            List<string> geolocation = FetchGeolocation("Plovdiv");
            Dictionary<string, double> dronesWindResist = DictionaryInitializer();

            if (!geolocation.Any())
            {
                ViewData["error"] =
                    "Please be sure that the name of the city that you insert is valid and try again!";
            };

            var url = $"https://api.openweathermap.org/data/2.5/weather?lat={geolocation[0]}&lon={geolocation[1]}&units=metric&APPID={_ApiKey}";
            var client = new WebClient();

            var body = client.DownloadString(url);

            JObject data = JObject.Parse(body);
            int mainTempInCelsius = (int)(data["main"]["temp"]);
            double visibilityInKm = (double)data["visibility"] / 1000;
            // double windSpeed = (double)data["wind"]["speed"];
            double windSpeed = 15;


            ViewData["temp"] = mainTempInCelsius;
            ViewData["city"] = data["name"];
            ViewData["country"] = data["sys"]["country"];
            ViewData["weather"] = data["weather"][0]["main"];
            ViewData["humidity"] = data["main"]["humidity"];
            ViewData["wind"] = windSpeed;
            ViewData["visibility"] = visibilityInKm;
            ViewData["atmPressure"] = data["main"]["pressure"];

            var temp = dronesWindResist["DJI Mini 2 SE"];

            ViewData["alert"] = (temp <= windSpeed)
                ?
                 $"Fly with caution! Your drone has wind resist of" +
                    $" {temp} m/s but the wind in the region is {windSpeed} m/s"

                    : "Flying your drone is completely safe!";


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

            var info1 = (Math.Round((double)data[0]["lat"], 2));
            var info2 = (Math.Round((double)data[0]["lon"], 2));

            List<string> geolocations = new List<string>();
            geolocations.Add(info1.ToString());
            geolocations.Add(info2.ToString());


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
    }
}