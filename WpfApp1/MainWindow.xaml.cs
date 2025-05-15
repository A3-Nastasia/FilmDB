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
            get_all_notes_db("");
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

        private void get_all_notes_db(string plus_sql)
        {
            using (var con = new SQLiteConnection(dB_Class.db_path))
            {
                con.Open();
                using (var cmd = new SQLiteCommand(con))
                {
                    cmd.CommandText = @"SELECT * FROM Films "+plus_sql;

                    SQLiteDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        UC_film ucFilm = new UC_film();
                        // присваиваем значения полям User Control'a из базы данных
                        ucFilm.tb_FilmName.Text = reader.GetString(1);
                        ucFilm.tb_FilmYear.Text = reader.GetInt32(2).ToString();

                        string imagePath = Add_Film.curDirectoryProject_images + "\\" + reader.GetString(1) + ".jpg";
                        BitmapImage bitmap_default;

                        if (File.Exists(imagePath))
                        {
                            bitmap_default = new BitmapImage(new Uri(imagePath));
                        }
                        else
                        {
                            bitmap_default = new BitmapImage(new Uri(Add_Film.curDirectoryProject_main + "\\default_img.jpg"));
                        }
                        ucFilm.UC_image.Source = bitmap_default;


                        string tb_default = ucFilm.tb_FilmGanre.Text;
                        ucFilm.tb_FilmGanre.Text = "";
                        //заполнение жанров через ,
                        using (var cmd2 = new SQLiteCommand(con))
                        {
                            cmd2.CommandText = @"SELECT Genres.name FROM FilmGenres JOIN Genres ON FilmGenres.genre_id = Genres.id WHERE FilmGenres.film_id='" + reader.GetInt32(0) + "'";

                            SQLiteDataReader reader_genres = cmd2.ExecuteReader();
                            StringBuilder genresBuilder = new StringBuilder();
                            while (reader_genres.Read())
                            {
                                if (genresBuilder.Length > 0)
                                    genresBuilder.Append(", ");
                                genresBuilder.Append(reader_genres.GetString(0));
                            }
                            ucFilm.tb_FilmGanre.Text = genresBuilder.ToString();

                            reader_genres.Close();
                            cmd2.ExecuteNonQuery();
                        }
                        wrapPanel_films.Children.Add(ucFilm);
                    }
                    reader.Close();
                    cmd.ExecuteNonQuery();
                }
                con.Close();
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
            get_all_notes_db("");
        }

        private void tb_search_TextChanged(object sender, TextChangedEventArgs e)
        {
            deleteAllUC();
            get_all_notes_db("WHERE title LIKE '%"+tb_search.Text+ "%' OR release_year LIKE '%"+tb_search.Text+"%'");
        }
    }
}