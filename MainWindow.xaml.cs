using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Xml.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Xml;
using System.Net;


namespace WeatherHelper
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private OpenWeatherMapClient owm;

        //private const string apiUrl = "http://api.openweathermap.org/data/2.5/weather?q={0}&units=metric&appid={1}";
        string apiKey = "ae157399507a8eb91962b38584164cf6";


        List<string> summerTop = new List<string> { "майка", "топ", "футболка" };
        List<string> summerBottom = new List<string> { "шорты", "юбка", "легкие брюки" };
        List<string> summerFull = new List<string> { "комбинезон", "платье" };

        List<string> warmTop = new List<string> { "рубашка", "блуза", "свитшот" };
        List<string> warmBottom = new List<string> { "брюки", "джинсы", "легкие брюки" };
        List<string> warmFull = new List<string> { "комбинезон", "платье", "спортивный костюм" };

        List<string> normTop = new List<string> { "утепленная рубашка", "блуза", "футболка с джинсовой курткой" };
        List<string> normBottom = new List<string> { "брюки", "джинсы", "юбка" };
        List<string> normFull = new List<string> { "спортивный костюм" };

        List<string> coolTop = new List<string> { "рубашка", "водолазка", "свитер" };
        List<string> coolBottom = new List<string> { "брюки", "джинсы" };
        List<string> coolOuter = new List<string> { "кожаная куртка", "плащ", "пальто" };

        List<string> coldOuter = new List<string> { "дублёнка", "тёплое пальто", "эко-шуба" };

        List<string> winterTop = new List<string> { "свитер", "флисовая кофта", "толстовка", "водолазка" };
        List<string> winterBottom = new List<string> { "спортивки", "джинсы" };
        List<string> winterOuter = new List<string> { "пуховик", "дубленка", "шуба" };


        double Temp_FeelsLike;
        double WindSpeed;
        string CloudStatus;
        double Humidity;


        public MainWindow()
        {
            InitializeComponent();
            string fileName1 = "cities.xml";
            string filePath1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName1);
            XmlDocument xmlDoc1;

            if (!File.Exists(filePath1))
            {
                xmlDoc1 = new XmlDocument();
                XmlDeclaration xmlDeclaration = xmlDoc1.CreateXmlDeclaration("1.0", "UTF-8", null);
                xmlDoc1.AppendChild(xmlDeclaration);

                XmlElement rootElement = xmlDoc1.CreateElement("cities");
                xmlDoc1.AppendChild(rootElement);

                xmlDoc1.Save(filePath1);
            }
            else
            {
                LoadFromXml();
            }
            listBox.Visibility = Visibility.Collapsed;


        }

       
        

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            if (textBox.Text.Length == 0)
            {
                MessageBox.Show("Введите название населенного пункта!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
                
            }

            string name_city = textBox.Text;

            ParseWeather(name_city);

            

            Recommendation();



        }

        private void ParseWeather(string name_city)
        {
            try
            {
                string url = $"http://api.openweathermap.org/data/2.5/weather?q={name_city}&lang=ru&appid={apiKey}";
                using (WebClient client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    string json = client.DownloadString(url);

                    dynamic result = JsonConvert.DeserializeObject(json);

                    double temperature = result.main.temp - 273.15; 
                    double windSpeed = result.wind.speed;
                    int humidity = result.main.humidity;
                    string cloudiness = result.weather[0].description.ToString();
                    double feelsLike = result.main.feels_like - 273.15;

                    tempLabel.Content = $"{Math.Round(temperature,2)} °C";
                    wind_humLabel.Content = $"Ветер: {windSpeed} м/с. \nВлажность: {humidity}%";
                    feelsLabel.Content = $"Ощущается как: {Math.Round(feelsLike,2)} °C";
                    statusLabel.Content = $"В городе {name_city} сейчас {cloudiness}";

                    Temp_FeelsLike = feelsLike;
                    WindSpeed = windSpeed;
                    CloudStatus = cloudiness;
                    Humidity = humidity;

                    AddNewCity(name_city);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения данных о погоде: {ex.Message}");
            }
        }


        private void LoadFromXml()
        {

            XDocument doc = XDocument.Load("cities.xml");

            foreach (var element in doc.Descendants("city"))
            {
                string area = element.Element("name")?.Value;

                if (!string.IsNullOrEmpty(area))
                {
                    ListBoxItem item = new ListBoxItem();
                    item.Content = area;

                    listBox.Items.Insert(0, item); 
                }
            }


        }

        private void AddNewCity(string place)
        {
            XDocument xmlDoc = XDocument.Load("cities.xml");
            XElement rootElement = xmlDoc.Element("cities");

            
            XElement cityElement = new XElement("city",
                new XElement("name", place));

            
            rootElement.Add(cityElement);
            xmlDoc.Save("cities.xml");

            
            ListBoxItem item = new ListBoxItem();
            item.Content = place;
            listBox.Items.Insert(0, item); 

        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (HistoryButton.Content.ToString() == "История запросов")
            {
                listBox.Visibility = Visibility.Visible;
                HistoryButton.Content = "Скрыть историю";
            }
            else
            { 
                listBox.Visibility = Visibility.Collapsed;
                HistoryButton.Content = "История запросов";
            }
                

        }

        private void Recommendation()
        {
            if (Temp_FeelsLike > 25)
            {
                string outfitDescription = $"Жарааа... Идеально подойдет сюда {randomChoice(summerTop)}, {randomChoice(summerBottom)} или {randomChoice(summerFull)}.";
                outputLabel.Text = outfitDescription;
            }
            else if (20 <= Temp_FeelsLike && Temp_FeelsLike <= 25)
            {
                if (WindSpeed < 8 && (CloudStatus == "ясно" || CloudStatus == "переменная облачность"))
                {
                    string outfitDescription = $"Очень тепло! Идеально подойдет сюда {randomChoice(summerTop)}, {randomChoice(warmBottom)} или {randomChoice(summerFull)}.";
                    outputLabel.Text = outfitDescription;
                }
                else
                {
                    string outfitDescription = $"Тепло! Идеально подойдет сюда {randomChoice(warmTop)}, {randomChoice(warmBottom)} или {randomChoice(warmFull)}.";
                    outputLabel.Text = outfitDescription;
                }
            }

            else if (13 < Temp_FeelsLike && Temp_FeelsLike < 20)
            {
                if (WindSpeed < 8 && CloudStatus == "ясно")
                {
                    string outfitDescription = $"Комфортная погода! Идеально подойдет сюда {randomChoice(normTop)}, {randomChoice(normBottom)} или {randomChoice(normFull)}.";
                    outputLabel.Text = outfitDescription;
                }
                else
                {
                    string outfitDescription = $"Комфортная погода! Идеально подойдет сюда {randomChoice(coolTop)}, {randomChoice(normBottom)} или {randomChoice(normFull)}.";
                    outputLabel.Text = outfitDescription;
                }
            }
            else if (7 < Temp_FeelsLike && Temp_FeelsLike <= 13)
            {
                string outfitDescription = $"Довольно прохладно. Идеально подойдет сюда {randomChoice(coolTop)}, {randomChoice(coolBottom)}, а в качестве верхней одежды будет {randomChoice(coolOuter)}.";
                outputLabel.Text = outfitDescription;
            }
            else if (0 <= Temp_FeelsLike && Temp_FeelsLike <= 7)
            {
                if (WindSpeed < 8 && CloudStatus == "ясно")
                {
                    string outfitDescription = $"Холодновато. В таком случае, чтобы не замерзнуть, подойдет {randomChoice(coolTop)}, " +
                        $"{randomChoice(coolBottom)}, а в качестве верхней одежды будет {randomChoice(coolOuter)}. И не забудьте надеть шарф!";
                    outputLabel.Text = outfitDescription;
                }
                else
                {
                    string outfitDescription = $"Холодновато. В таком случае, чтобы не замерзнуть, подойдет {randomChoice(coolTop)},  {randomChoice(coolBottom)}, " +
                        $"а в качестве верхней одежды будет {randomChoice(coldOuter)}.";
                    outputLabel.Text = outfitDescription;
                }
            }
            else if (-10 <= Temp_FeelsLike && Temp_FeelsLike <= 0)
            {
                if (WindSpeed < 10 && CloudStatus == "ясно" && Humidity < 70)
                {
                    string outfitDescription = $"Холодно. В таком случае, чтобы не замерзнуть, подойдет {randomChoice(coolTop)}, a {randomChoice(coolBottom)}, " +
                        $"а в качестве верхней одежды будет {randomChoice(coldOuter)}. Пора надевать шапку!";
                    outputLabel.Text = outfitDescription;
                }
                else
                {
                    string outfitDescription = $"Уже по-зимнему холодно! В таком случае, чтобы не замерзнуть, подойдет {randomChoice(coolTop)}, " +
                        $"{randomChoice(coolBottom)}, а в качестве верхней одежды будет {randomChoice(coldOuter)}. Пора надевать шапку и колготки/подштанники!";
                    outputLabel.Text = outfitDescription;
                }
            }

            else if (-10 <= Temp_FeelsLike && Temp_FeelsLike <= 0)
            {
                if (WindSpeed < 10 && CloudStatus == "ясно" && Humidity < 70)
                {
                    string outfitDescription = $"Холодно. В таком случае, чтобы не замерзнуть, подойдет {randomChoice(coolTop)}, a {randomChoice(coolBottom)}, " +
                        $"а в качестве верхней одежды будет {randomChoice(coldOuter)}. Пора надевать шапку!";
                    outputLabel.Text = outfitDescription;
                }
                else
                {
                    string outfitDescription = $"Уже по-зимнему холодно! В таком случае, чтобы не замерзнуть, подойдет {randomChoice(coolTop)}, " +
                        $"{randomChoice(coolBottom)}, а в качестве верхней одежды будет {randomChoice(coldOuter)}. Пора надевать шапку и колготки/подштанники!";
                    outputLabel.Text = outfitDescription;
                }
            }

            else if (Temp_FeelsLike < -10)
            {
                if (WindSpeed < 8 && CloudStatus == "ясно" && Humidity < 70)
                {
                    string outfitDescription = $"Морозно. В таком случае, чтобы не замерзнуть, подойдет {randomChoice(winterTop)}, a {randomChoice(winterBottom)}, " +
                        $"а в качестве верхней одежды будет {randomChoice(winterOuter)}. Не забудьте про тёплые носки и термобелье под одежду!";
                    outputLabel.Text = outfitDescription;
                }
                else
                {
                    string outfitDescription = $"Пробирающий до костей мороз... В таком случае, чтобы не замерзнуть, подойдет {randomChoice(winterTop)}, " +
                        $"{randomChoice(winterBottom)}, а в качестве верхней одежды будет {randomChoice(winterOuter)}. Укутывайтесь до ушей, потеплее и побольше!";
                    outputLabel.Text = outfitDescription;
                }
            }

            if (WindSpeed > 15)
            {
                outputLabel.Text = "На улице сильный ветер, сегодня лучше посидеть дома.";
            }


        }

        Random random = new Random();

        private string randomChoice(List<string> list)
        {
            return list[random.Next(list.Count)];
        }


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                searchButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        private void listBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem selectedCity = listBox.SelectedItem as ListBoxItem;
            
            if (selectedCity != null)
            {
                string scity = selectedCity.Content.ToString();
                ParseWeather(scity);
                Recommendation();
                textBox.Text = scity;
            }

            

            
        }
    }
}
