﻿using System;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace Cafeteria.Wpf.Infrastructure
{
    public class UiService : IUiService
    {
        public T GetView<T>() where T : new()
        {
            var newWindow = (T)Activator.CreateInstance(typeof(T));

            var window = newWindow as Window;

            if (window != null)
            {
                if (Application.Current.MainWindow != window)
                {
                    window.Owner = Application.Current.MainWindow;
                }
            }
            return newWindow;
        }

        public async Task<MessageDialogResult> ShowYesNoQuestionAsync(string question, string title)
        {
            var window = Application.Current.MainWindow as MetroWindow;
            window.MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Accented;
            return await window.ShowMessageAsync(title, question, MessageDialogStyle.AffirmativeAndNegative, window.MetroDialogOptions);
        }
    }
}
