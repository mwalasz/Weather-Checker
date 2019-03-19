using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WindowWeatherApp
{
    public partial class MainWindow : Window
    {

        protected const int NUM_OF_WEATHER_DATA = 5;
        protected const string FILE_PATH = "C:\\Users\\Mateusz\\Desktop\\Weather_GIG.txt";
        protected const string URL_PATH = "http://meteo.gig.eu/";

        public static string windDirection = "";
        public static string archiveWindDirection = "";

        private static Object[,] weatherData = new Object[NUM_OF_WEATHER_DATA, 5];

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

        private static double[] archiveDataArray = new double[NUM_OF_WEATHER_DATA];
        public static double[] ArchiveDataArray
        {
            get { return archiveDataArray; }
            set { archiveDataArray = value; }
        }

        private static string[] finalWeatherInfoToDisplay = new String[NUM_OF_WEATHER_DATA];
        public static string[] FinalWeatherInfoToDisplay
        {
            get { return finalWeatherInfoToDisplay; }
            set { finalWeatherInfoToDisplay = value; }
        }
        public readonly string[] labelTextsArray = { "Temperatura", "Wilgotność", "Opady", "Wiatr", "Ciśnienie" };

        private static readonly string[] unitsArray = { "°C", " %", " mm", "m/s", " hPa" };
        public static string[] UnitsArray { get { return unitsArray; } }

        //------------------------------------------------------------------------------------------------------

        private static void SetUpdateTime()
        {
            timeOfUpdate = DateTime.Now;
        }

        private static string GetDataFromWebSite(string url, string filePath)
        {
            SetUpdateTime();

            WebClient client = new WebClient();
            Stream data = client.OpenRead(url);
            StreamReader reader = new StreamReader(data);

            string htmlCodeFromWebSite = reader.ReadToEnd();

            data.Close();
            reader.Close();

            File.WriteAllText(filePath, htmlCodeFromWebSite);

            return GetTextBetweenWords(htmlCodeFromWebSite, "<td class=\"czynnik\">Temperatura</td><td class=\"wartosc\">", "</td></tr><tr><td class=\"czynnik\">Intensywno");
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
            string firstSequenceToFind = "</td><td class=\"wartosc\">&nbsp;", secondSequenceToFind = "&nbsp;", interspace = ": ", searchData, temp, temp2 = whatToSearchFor;

            if (whatToSearchFor == "Temperatura")
            {
                searchData = GetTextBetweenWords(textFromWeb, secondSequenceToFind, secondSequenceToFind);
                arr = whatToSearchFor + interspace + searchData + " " + unit;
                whatToSearchFor = temp2;
                return Convert.ToDouble(searchData);
            }

            if (whatToSearchFor == "Opady")
                whatToSearchFor = whatToSearchFor.ToLower();

            if (whatToSearchFor == "Wiatr")
            {
                secondSequenceToFind = " m/s</td>";
                temp = whatToSearchFor + firstSequenceToFind;
                searchData = GetTextBetweenWords(textFromWeb, temp, secondSequenceToFind);
                windDirection = searchData.Substring(0, searchData.IndexOf('p')-1);
                temp = searchData.Substring( searchData.IndexOf('y') + 1);
                searchData = temp;
                whatToSearchFor = temp2;

                return Convert.ToDouble(searchData);
            }

            if (whatToSearchFor == "Ciśnienie")
                whatToSearchFor = "stacji)";

            temp = whatToSearchFor + firstSequenceToFind;
            searchData = GetTextBetweenWords(textFromWeb, temp, secondSequenceToFind);

            if (whatToSearchFor == "stacji)")
                whatToSearchFor = "Ciśnienie";

            //zamiana pierwszej litery na duza

            whatToSearchFor = temp2; //przywrocenie pierwotnej wartosci
            return Convert.ToDouble(searchData);
        }

        public static void DownloadWeatherInfo( double[] data, string[] labels, string[] units, string[] finalArr, ref string wind)
        {
            try
            {
                var weatherData = "";
                weatherData = GetDataFromWebSite(URL_PATH, FILE_PATH);
                //DownloadWeatherInfo(data, labels, units, finalArr, wind);
            
                for (int i = 0; i < NUM_OF_WEATHER_DATA; i++)
                {
                     data[i] = GetEachValueFromText(weatherData, labels[i], units[i], ref finalArr[i], ref wind);
                }
            }
            catch (System.Net.WebException e1)
            {
                MessageBox.Show("Blad podczas otwierania strony!");
            }
            catch (System.IO.DirectoryNotFoundException e2)
            {
                MessageBox.Show("Bledna sciezka do pliku!");
            }
        }

        private void ArchiveWeatherDataBeforeUpdate()
        {
            for (int i = 0; i < NUM_OF_WEATHER_DATA; i++)
            {
                ArchiveDataArray[i] = DataArray[i];
            }
            archiveWindDirection = windDirection;
        }

        private void UpdateTextBoxesData()
        {
            Temp.Text = DataArray[0].ToString();
            Wilg.Text = DataArray[1].ToString();
            Opad.Text = DataArray[2].ToString();
            Wiatr.Text = DataArray[3].ToString();
            Cisn.Text = DataArray[4].ToString();

            if (DataArray[3] > 0) //jesli wiatr wieje, to ma kierunek
                Kierunek.Text = windDirection;
            else Kierunek.Text = "--";

            UpdateTime.Text = TimeOfUpdate.ToString();
        }

        private void ChangeColorOfWeatherDataText()
        {
            if (DataArray[0] == ArchiveDataArray[0])
                Temp.Foreground = new SolidColorBrush(Colors.Crimson);
            else Temp.Foreground = new SolidColorBrush(Colors.Lime);

            if (DataArray[1] == ArchiveDataArray[1])
                Wilg.Foreground = new SolidColorBrush(Colors.Crimson);
            else Wilg.Foreground = new SolidColorBrush(Colors.Lime);

            if (DataArray[2] == ArchiveDataArray[2])
                Opad.Foreground = new SolidColorBrush(Colors.Crimson);
            else Opad.Foreground = new SolidColorBrush(Colors.Lime);

            if (DataArray[3] == ArchiveDataArray[3])
                Wiatr.Foreground = new SolidColorBrush(Colors.Crimson);
            else Wiatr.Foreground = new SolidColorBrush(Colors.Lime);

            if (DataArray[4] == ArchiveDataArray[4])
                Cisn.Foreground = new SolidColorBrush(Colors.Crimson);
            else Cisn.Foreground = new SolidColorBrush(Colors.Lime);
       
            if (windDirection == archiveWindDirection)
                Kierunek.Foreground = new SolidColorBrush(Colors.Crimson);
            else Kierunek.Foreground = new SolidColorBrush(Colors.Lime);
        }

        public void UpdateData()
        {
            ArchiveWeatherDataBeforeUpdate();

            DownloadWeatherInfo(DataArray, labelTextsArray, UnitsArray, FinalWeatherInfoToDisplay, ref windDirection);
            
            UpdateTextBoxesData();

            ChangeColorOfWeatherDataText();
        }
        
        private void UpdateButton_OnClick(object sender, RoutedEventArgs e)
        {
            UpdateData();
        }

        public MainWindow()
        {
            InitializeComponent();
            UpdateData();
        }
    }
}