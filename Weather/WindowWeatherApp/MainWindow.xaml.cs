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

//TODO dodac wyswietlanie minmax
//TODO zapis do pliku minmax
//TODO uporzadkowanie kodu
//TODO usunac zalegle stare zmienne(finalARR, dataArray)

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
        public static readonly string[] labelTextsArray = { "Temperatura", "Wilgotność", "Opady", "Wiatr", "Ciśnienie" };

        private static readonly string[] unitsArray = { "°C", " %", " mm", "m/s", " hPa" };
        public static string[] UnitsArray { get { return unitsArray; } }

        //------------------------------------------------------------------------------------------------------

        private static void ChangesValuesOfMultiDimArray(ref Object[,] arr, int row)
        {
            if (arr[row, 1] == null || arr[row, 3] == null)
            {
                arr[row, 1] = arr[row, 3] = arr[row, 0];
                arr[row, 2] = arr[row, 4] = timeOfUpdate;
            }
            else
            {
                if ((double)arr[row, 0] < (double)arr[row, 1])
                {
                    arr[row, 1] = arr[row, 0]; //ustawienie nowego minimum
                    arr[row, 2] = TimeOfUpdate;
                }
                else if ((double)arr[row, 0] > (double)arr[row, 3])
                {
                    arr[row, 1] = arr[row, 3]; //ustawienie nowego maksimum
                    arr[row, 4] = TimeOfUpdate;
                }
            }
        }

        private static void SetMinMaxOfData(ref Object[,] arr, string nameOfData)
        {
            var index = Array.IndexOf(labelTextsArray, nameOfData);
            ChangesValuesOfMultiDimArray(ref arr, index);
        }

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

            whatToSearchFor = temp2; //przywrocenie pierwotnej wartosci
            return Convert.ToDouble(searchData);
        }

        public static void DownloadWeatherInfo( Object[,] data, string[] labels, string[] units, string[] finalArr, ref string wind)
        {
            try
            {
                var weatherData = "";
                weatherData = GetDataFromWebSite(URL_PATH, FILE_PATH);
            
                for (int i = 0; i < NUM_OF_WEATHER_DATA; i++)
                {
                     data[i,0] = GetEachValueFromText(weatherData, labels[i], units[i], ref finalArr[i], ref wind);
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
                if (weatherData[i, 0] == null)
                    ArchiveDataArray[i] = 0;
                else ArchiveDataArray[i] = (double)weatherData[i, 0];
            }
            archiveWindDirection = windDirection;
        }

        private void UpdateTextBoxesData()
        {
            Temp.Text = weatherData[0,0].ToString();
            Wilg.Text = weatherData[1, 0].ToString();
            Opad.Text = weatherData[2, 0].ToString();
            Wiatr.Text = weatherData[3, 0].ToString();
            Cisn.Text = weatherData[4, 0].ToString();

            if ((double)weatherData[3, 0] > 0) //jesli wiatr wieje, to ma kierunek
                Kierunek.Text = windDirection;
            else Kierunek.Text = "--";

            UpdateTime.Text = TimeOfUpdate.ToString();
        }

        private void ChangeColorOfObject(TextBox tb, int index)
        {
            if ((double)weatherData[index, 0] == ArchiveDataArray[index])
                tb.Foreground = new SolidColorBrush(Colors.Crimson);
            else tb.Foreground = new SolidColorBrush(Colors.Lime);
        }

        private void ChangeColorOfWeatherDataText()
        {
            ChangeColorOfObject(Temp, 0);
            ChangeColorOfObject(Wilg, 1);
            ChangeColorOfObject(Opad, 2);
            ChangeColorOfObject(Wiatr, 3);
            ChangeColorOfObject(Cisn, 4);
        }

        public void UpdateData()
        {
            ArchiveWeatherDataBeforeUpdate();

            DownloadWeatherInfo(weatherData, labelTextsArray, UnitsArray, FinalWeatherInfoToDisplay, ref windDirection);

            foreach (var str in labelTextsArray) //dla wszystkich wlasciwosci pogody
                SetMinMaxOfData(ref weatherData, str);

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