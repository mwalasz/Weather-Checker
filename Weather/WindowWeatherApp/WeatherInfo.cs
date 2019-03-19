using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace WindowWeatherApp
{
    class WeatherInfo : Window
    {
        protected const int NUM_OF_WEATHER_DATA = 5;
        protected const string FILE_PATH = "C:\\Users\\Mateusz\\Desktop\\Weather_GIG.txt";
        protected const string URL_PATH = "http://meteo.gig.eu/";

        private static string windDirection = "";
        public static string WindDirection
        {
            get { return windDirection; }
            set { windDirection = value; }
        }

        private static DateTime timeOfUpdate;
        public static DateTime TimeOfUpdate
        {
            get { return timeOfUpdate; }
            set { timeOfUpdate = value; }
        }

        private static double[] dataArray = new double[NUM_OF_WEATHER_DATA];
        public static double[] DataArray
        {
            get { return dataArray; }
            set { dataArray = value; }
        }

        private static string[] finalWeatherInfoToDisplay = new String[NUM_OF_WEATHER_DATA];
        public static string[] FinalWeatherInfoToDisplay
        {
            get { return labelTextsArray; }
            set { finalWeatherInfoToDisplay = value; }
        }

        private static string[] labelTextsArray = { "Temperatura", "Wilgotność", "Opady", "Wiatr", "Ciśnienie" };
        public static string[] LabelTextsArray { get { return labelTextsArray; }}

        private static string[] unitsArray = { "°C", " %", " mm", "m/s", " hPa" };
        public static string[] UnitsArray { get { return unitsArray; }}

        //------------------------------------------------------------------------------------------------------

        private static void SetUpdateTime()
        {
            timeOfUpdate = DateTime.Now;
        }

        private static string GetDataFromWebSite(string url, string filePath)
        {
            try
            {
                WebClient client = new WebClient();
                Stream data = client.OpenRead(url);
                StreamReader reader = new StreamReader(data);

                string htmlCodeFromWebSite = reader.ReadToEnd();

                data.Close();
                reader.Close();

                SetUpdateTime();
                File.WriteAllText(filePath, htmlCodeFromWebSite);
                return GetTextBetweenWords(htmlCodeFromWebSite, "<td class=\"czynnik\">Temperatura</td><td class=\"wartosc\">", "</td></tr><tr><td class=\"czynnik\">Intensywno");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                throw;
            }
        }

        private static string GetTextBetweenWords(string strSource, string strStart, string strEnd)
        {
            int start = 0, end = 0;
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                start = strSource.IndexOf(strStart, 0) + strStart.Length;
                end = strSource.IndexOf(strEnd, start);
                return strSource.Substring(start, end - start);
            }
            else return "";
        }

        public static double GetEachValueFromText(string textFromWeb, string whatToSearchFor, string unit, ref string arr, ref string windDirection)
        {
            string firstSequenceToFind = "</td><td class=\"wartosc\">&nbsp;", secondSequenceToFind = "&nbsp;", interspace = ": ", searchData, temp;

            if (whatToSearchFor == "Temperatura")
            {
                searchData = GetTextBetweenWords(textFromWeb, secondSequenceToFind, secondSequenceToFind);
                arr = whatToSearchFor + interspace + searchData + " " + unit;
                return Convert.ToDouble(searchData);
            }

            if (whatToSearchFor == "Opady")
                    whatToSearchFor = whatToSearchFor.ToLower();

            if (whatToSearchFor == "Wiatr")
            {
                secondSequenceToFind = " m/s</td>";
                temp = whatToSearchFor + firstSequenceToFind;
                searchData = GetTextBetweenWords(textFromWeb, temp, secondSequenceToFind);
                windDirection = searchData.Substring(0, searchData.IndexOf('y') + 1);
                searchData = searchData.Substring(searchData.IndexOf('y') + 1, 4);
                arr = whatToSearchFor + interspace + windDirection + searchData + " " + unit;

                return Convert.ToDouble(searchData);
            }

            if (whatToSearchFor == "Ciśnienie")
                whatToSearchFor = "stacji)";

            temp = whatToSearchFor + firstSequenceToFind;
            searchData = GetTextBetweenWords(textFromWeb, temp, secondSequenceToFind);

            if (whatToSearchFor == "stacji)")
                whatToSearchFor = "Ciśnienie";

            arr = (whatToSearchFor.First().ToString().ToUpper() + whatToSearchFor.Substring(1)) + interspace + searchData + unit;
                  //zamiana pierwszej litery na duza
            return Convert.ToDouble(searchData);
        }

        public static void DisplayWeatherInfo(string weatherData, double[] data, string[] labels, string[] units, string[] finalArr, string wind)
        {
            Console.Clear();
            Console.WriteLine("Data aktualizacji: " + Convert.ToString(timeOfUpdate) + "\n");

            for (int i = 0; i < NUM_OF_WEATHER_DATA; i++)
            {
                data[i] = GetEachValueFromText(weatherData, labels[i], units[i], ref finalArr[i], ref wind);
                Console.WriteLine(finalArr[i]);
            }

            Console.WriteLine();
        }

        public void DownloadWeatherInfo(double[] data, string[] labels, string[] units,
            string[] finalArr, string wind)
        {
            try
            {
                var weatherData = GetDataFromWebSite(URL_PATH, FILE_PATH);
                DisplayWeatherInfo(weatherData, data, labels, units, finalArr, wind);
            }
            catch (System.Net.WebException e1)
            {
                Console.WriteLine("Blad podczas otwierania strony!");
            }
            catch (System.IO.DirectoryNotFoundException e2)
            {
                Console.WriteLine("Bledna sciezka do pliku!");
            }
        }
    }
}
