﻿using System;
using System.Linq;
using Learner.Models;
using Microsoft.EntityFrameworkCore;
using Xamarin.Forms;

namespace Learner
{
    public partial class EntryPage : ContentPage
    {
        private Word _word;

        private bool isEditing = false;

        public EntryPage()
        {
            InitializeComponent();

            Title = "Word adding";

            if (App._collections != null && App._collections.Count > 0)
            {
                var control = new Picker
                {
                    Title = "Collection",
                    ItemsSource = App._collections.Select(x => x.Name).ToList(),
                    ClassId = "picker2"
                };
                stackLayout.Children.Insert(4, control);
            }
        }

        public EntryPage(Word word)
        {
            InitializeComponent();

            Title = "Word editing";

            _word = word;
            wordText.Text = word.Text;
            transcription.Text = word.Transcription;
            translation.Text = word.Translation;
            isEditing = true;
            var item = new ToolbarItem { Text = "🗑" };
            item.Clicked += OnDeleteClicked;
            ToolbarItems.Add(item);
            picker1.SelectedItem = picker1.Items.FirstOrDefault(x => x == word.Language);
        }

        async void OnSaveButtonClicked(object sender, EventArgs e)
        {
            if (wordText.Text == null || translation.Text == null || picker1.SelectedIndex == -1)
            {
                await DisplayAlert("Alert!", "Fill all the fields!", "Ok");
                return;
            }

            var picker = stackLayout.Children.FirstOrDefault(x => x.ClassId == "picker2") as Picker;

            if (_word == null)
            {
                _word = new Word
                {
                    Id = Guid.NewGuid(),
                };
            }

            _word.Text = wordText.Text;
            _word.Transcription = transcription.Text;
            _word.Translation = translation.Text;
            _word.Language = picker1.SelectedItem.ToString();

            if (isEditing)
            {
                App.Context.Words.Update(_word);
            }
            else
            {
                App.Context.Add(_word);
            }

            if (picker != null)
            {
                if (picker.SelectedIndex != -1)
                {
                    var col = await App.Context.Collections.FirstOrDefaultAsync(x =>
                        x.Name == picker.ItemsSource[picker.SelectedIndex].ToString());

                    if (col != null)
                    {
                        col.Words.Add(_word);

                        var result = App.Context.Collections.Update(col);
                    }
                }
            }

            await App.Context.SaveChangesAsync();

            await Navigation.PopAsync();
        }

        async void OnCancelButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        async void OnDeleteClicked(object sender, EventArgs e)
        {
            var result = await DisplayAlert("Delete this item?", "This is permanent and cannot be undone.", "Delete", "Cancel");

            if (!result)
                return;

            App.Context.Words.Remove(_word);

            await App.Context.SaveChangesAsync();

            await Navigation.PopAsync();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if(!isEditing)
                wordText.Focus();
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
                    Grid.SetRow(button2, 0);
                    Grid.SetColumn(button2, 1);

                    Grid.SetColumnSpan(button1, 1);
                    Grid.SetColumnSpan(button2, 1);
                }
                else
                {
                    Grid.SetRow(button2, 1);
                    Grid.SetColumn(button2, 0);

                    Grid.SetColumnSpan(button1, 2);
                    Grid.SetColumnSpan(button2, 2);
                }
            }
        }
    }
}
