﻿using System;
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
using System.Windows.Shapes;
using WpfApp1.DB;

namespace WpfApp1.MoreWindows
{
    /// <summary>
    /// Логика взаимодействия для Settings_MainWindow.xaml
    /// </summary>
    public partial class Settings_MainWindow : Window
    {
        DB_class dB_Class = new DB_class();
        public Settings_MainWindow()
        {
            InitializeComponent();
        }

        private void btn_Clear_Click(object sender, RoutedEventArgs e)
        {
            dB_Class.clear_all_tables();
        }

        private void btn_Clear_film_Click(object sender, RoutedEventArgs e)
        {
            dB_Class.clear_films_table();
        }
    }
}
