using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;
using WpfApp1.DB;
using WpfApp1.MoreWindows;

namespace WpfApp1.UserControls
{
    public partial class UC_film : UserControl
    {
        public UC_film()
        {
            InitializeComponent();
        }

        DB_class dB_Class = new DB_class();
        Add_Film add_Film = new Add_Film();
        public void delete_film()
        {
            using (var con = new SQLiteConnection(dB_Class.db_path))
            {
                con.Open();
                using (var cmd = new SQLiteCommand(con))
                {
                    cmd.CommandText = @"DELETE FROM Films WHERE title='" + tb_FilmName.Text + "'";

                    string directoryPath = Add_Film.curDirectoryProject_images + "\\";
                    string[] filesToDelete = Directory.GetFiles(directoryPath, (tb_FilmName.Text + ".jpg"));

                    cmd.ExecuteNonQuery();


                    foreach (string file in filesToDelete)
                    {
                        File.Delete(file);
                    }
                }
                con.Close();
            }
        }

        private void btn_delete_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы точно хотите удалить данную запись?", "Удаление записи", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result==MessageBoxResult.Yes)
            {
                delete_film();
                MessageBox.Show("Запись успешно удалена", "Удаление записи", MessageBoxButton.OK, MessageBoxImage.None);
            }
        }

        private void btn_correction_Click(object sender, RoutedEventArgs e)
        {
            add_Film.correcting_film(tb_FilmName, tb_FilmGanre, UC_image);
            add_Film.Show();
        }
    }
}
