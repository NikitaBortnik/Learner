using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Learner.Models;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Learner
{
    public partial class QuizPage : ContentPage
    {
        Guid rightButtonId;

        string hint;

        string answerLocale;

        private double width;
        private double height;

        public QuizPage()
        {
            InitializeComponent();

            button1.Clicked += SomeButtonClicked;
            button2.Clicked += SomeButtonClicked;
            button3.Clicked += SomeButtonClicked;
            button4.Clicked += SomeButtonClicked;

            StartQuiz();
        }

        void StartQuiz()
        {
            answersLabel.Text = App.Scores.ToString();

            Button[] buttonArray = { button1, button2, button3, button4 };

            Random rnd = new Random();
            List<Word> wordList = new List<Word>();

            //while instead normal roll
            while (wordList.Count < 4)
            {
                var word = App._words[rnd.Next(App._words.Count)];

                if (!wordList.Contains(word))
                {
                    buttonArray[wordList.Count].Text = word.Translation;
                    wordList.Add(word);
                }
            }

            int index = rnd.Next(buttonArray.Length);
            rightButtonId = buttonArray[index].Id;
            label1.Text = wordList[index].Text;
            hint = wordList[index].Transcription;
            answerLocale = wordList[index].Language;
        }

        //cast sender to button and compare the Id property with Id of "right" button
        void SomeButtonClicked(object sender, EventArgs e)
        {
            if (((Button)sender).Id == rightButtonId)
            {
                label1.Text = "Success!";
                ++App.Scores;
            }
            else
                label1.Text = "Not success";
            StartQuiz();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (width != this.width || height != this.height)
            {
                this.width = width;
                this.height = height;

                if (width > height) //horizontal
                {
                    Grid.SetRow(button2, 3);
                    Grid.SetRow(button3, 4);
                    Grid.SetRow(button4, 4);
                    Grid.SetColumn(button2, 1);
                    Grid.SetColumn(button4, 1);

                    Grid.SetColumnSpan(button1, 1);
                    Grid.SetColumnSpan(button2, 1);
                    Grid.SetColumnSpan(button3, 1);
                    Grid.SetColumnSpan(button4, 1);
                }
                else
                {
                    Grid.SetRow(button2, 4);
                    Grid.SetRow(button3, 5);
                    Grid.SetRow(button4, 6);
                    Grid.SetColumn(button2, 0);
                    Grid.SetColumn(button4, 0);

                    Grid.SetColumnSpan(button1, 2);
                    Grid.SetColumnSpan(button2, 2);
                    Grid.SetColumnSpan(button3, 2);
                    Grid.SetColumnSpan(button4, 2);
                }
            }
        }

        async void OnHelpClicked(object sender, EventArgs e)
        {
            if (await DisplayAlert("", "Use 10 points to show hint?", "Use", "Cancel")) //check the ! condition
            {
                if (App.Scores >= 10)
                {
                    App.Scores -= 10;
                    await DisplayAlert("", hint, "Close");
                    answersLabel.Text = App.Scores.ToString();
                }
                else
                    await DisplayAlert("", "Not enough points!", "Close");
            }
        }

        async void OnSpeachClicked(object sender, EventArgs e)
        {
            var locales = await TextToSpeech.GetLocalesAsync();

            // Grab the first locale
            var locale = locales.FirstOrDefault(x => x.Name.ToLower().Contains(answerLocale.ToLower()));

            var settings = new SpeechOptions
            {
                Volume = 1
            };

            if (locale != null)
                settings.Locale = locale;
            
            await TextToSpeech.SpeakAsync(label1.Text, settings);
        }
    }
}
