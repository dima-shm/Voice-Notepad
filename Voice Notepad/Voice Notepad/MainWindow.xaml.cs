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
using System.Windows.Shapes;
using Microsoft.Speech.Recognition; // Пространство имен голосового движка
using System.IO;

namespace Voice_Notepad
{
    public partial class MainWindow : Window
    {
        private SpeechRecognitionEngine speechRecEngine;
        private Choices comands;
        private GrammarBuilder grammBuilder;
        private Grammar gramm;
        private TextBox text;
        private TextBlock textBlock;
        private bool firstWord = true;


        public MainWindow()
        {
            InitializeComponent();
        }
        private void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Confidence > 0.6)
            {
                if (e.Result.Text == "сохранить")
                {
                    File.WriteAllText(@"C:\Users\dima-\Documents\Voice Notepad.rtf", text.Text);
                    textBlock.Text = ("Последнее сохранение: " + System.DateTime.Now.ToLongTimeString());
                }
                else if (e.Result.Text == "новая строка")
                {
                    text.Text += "\r\n";
                }
                else if (e.Result.Text == "точка")
                {
                    text.Text = text.Text.Substring(0, text.Text.Length - 1);
                    text.Text += ". ";
                    firstWord = true;
                }
                else if (e.Result.Text == "запятая")
                {
                    text.Text = text.Text.Substring(0, text.Text.Length - 1);
                    text.Text += ", ";
                }
                else if (e.Result.Text == "абзац")
                {
                    text.Text += "\t";
                }
                else if (e.Result.Text == "с большой буквы")
                {
                    firstWord = true;
                }
                else if (e.Result.Text == "стереть")
                {
                    text.Text = text.Text.Substring(0, text.Text.Length - 1);
                    firstWord = false;
                }
                else if (e.Result.Text == "стереть всё")
                {
                    text.Clear();
                    firstWord = true;
                }
                else if (e.Result.Text == "пробел")
                {
                    text.Text += " ";
                }
                else
                {
                    if (firstWord)
                    {
                        text.Text += FirstUpper(e.Result.Text);
                        text.Text += " ";
                        firstWord = false;
                    }
                    else
                    {
                        text.Text += e.Result.Text;
                        text.Text += " ";
                    }
                }
            }
            MoveCursor();
        }
        private void MoveCursor()
        {
            text.SelectionStart += text.Text.Length;
        }
        private string FirstUpper(string str)
        {
            string[] s = str.Split(' ');

            for (int i = 0; i < s.Length; i++)
            {
                if (s[i].Length > 1)
                    s[i] = s[i].Substring(0, 1).ToUpper() + s[i].Substring(1, s[i].Length - 1).ToLower();
                else
                    s[1] = s[1].ToUpper();
            }
            return string.Join(" ", s);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            text = textBox1;
            textBlock = timeSaved;
            text.Focus();                                                                                      // Курсор видимый по умолчанию


            speechRecEngine = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("ru-ru"));      // Инициализируем новый экземпляр класса для определенного языкового стандарта
            speechRecEngine.SetInputToDefaultAudioDevice();                                                    // Установка получения входных данных из аудиоустройства по умолчанию
            speechRecEngine.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(SpeechRecognized); // Определяем функию обработчик речи
            speechRecEngine.InitialSilenceTimeout = TimeSpan.FromMilliseconds(3);                              // Интервал ожидания полной тишины (без шума)


            comands = new Choices();                                                                           // Команды, подлежащие распознанию
            comands.Add(new string[] { "один", "два", "три", "четыре", "пять" , 
                                       "новая строка", "сохранить", "сохранить как", "точка", 
                                       "запятая", "абзац", "с большой буквы", 
                                       "стереть", "стереть всё" , "пробел" });


            speechRecEngine.UnloadAllGrammars();
            grammBuilder = new GrammarBuilder();
            grammBuilder.AppendWildcard();
            grammBuilder.Append(comands);

            gramm = new Grammar(grammBuilder);
            speechRecEngine.LoadGrammar(gramm);                                                                // Загружаем "грамматику"


            speechRecEngine.RecognizeAsync(RecognizeMode.Multiple);                                            // Запускаем распознавание
        }
    }
}
