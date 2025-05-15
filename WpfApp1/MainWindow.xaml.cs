using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WpfApp1.DB;
using WpfApp1.MoreWindows;
using WpfApp1.UserControls;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        DB_class dB_Class = new DB_class();
        WrapPanel wrapPanel_films = new WrapPanel();
        Add_Film add_Film = new Add_Film();

        public MainWindow()
        {
            InitializeComponent();

            wrapPanel_films.Margin = new Thickness(20, 25, 20, 25);
            wrapPanel_films.HorizontalAlignment = HorizontalAlignment.Center;

            stackPanel.Children.Add(wrapPanel_films);

            dB_Class.create_and_open();
            dB_Class.get_all_notes_db("");
        }


        private void deleteAllUC()
        {
            for (int i = this.wrapPanel_films.Children.Count - 1; i >= 0; i--)
            {
                UIElement element = this.wrapPanel_films.Children[i];
                if (element is Control)
                {
                    Control ctrl = (Control)element;
                    if (ctrl is UC_film)
                    {
                        this.wrapPanel_films.Children.Remove(ctrl);
                    }
                }
            }
        }


        private void btn_add_Click(object sender, RoutedEventArgs e)
        {
            Add_Film add_Film = new Add_Film();
            add_Film.Show();
        }

        private void btn_settings_Click(object sender, RoutedEventArgs e)
        {
            Settings_MainWindow settings_MainWindow = new Settings_MainWindow();
            settings_MainWindow.Show();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {

            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Shutdown();
            }
            else
            {
                System.Environment.Exit(1);
            }

            foreach (var process in Process.GetProcessesByName("WpfApp1.exe"))
            {
                process.Kill();
            }
        }

        private void btn_refresh_Click(object sender, RoutedEventArgs e)
        {
            deleteAllUC();
            dB_Class.get_all_notes_db("", wrapPanel_films);
        }

        private void tb_search_TextChanged(object sender, TextChangedEventArgs e)
        {
            deleteAllUC();
            dB_Class.get_all_notes_db("WHERE title LIKE '%"+tb_search.Text+ "%' OR release_year LIKE '%"+tb_search.Text+"%'", wrapPanel_films);
        }
    }
}