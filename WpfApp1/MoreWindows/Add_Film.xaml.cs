using Microsoft.Win32;
using System;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WpfApp1.DB;
using WpfApp1.UserControls;

namespace WpfApp1.MoreWindows
{
    public partial class Add_Film : Window
    {
        DB_class dB_Class = new DB_class();
        private static string curDirectory = Environment.CurrentDirectory;
        public static string curDirectoryProject_main = Directory.GetParent(curDirectory).Parent.FullName;
        public static string curDirectoryProject_images = curDirectoryProject_main + "\\images";


        public Add_Film()
        {
            InitializeComponent();

            btn_addFinish.Content = "Add";
            listBox_ganre_allFromDB();
            BitmapImage bitmap_default = new BitmapImage(new Uri(curDirectoryProject_main + "\\default_img.jpg"));
            img_drop.Source = bitmap_default;
        }


        public void correcting_film(TextBlock tb_FilmName, TextBlock tb_FilmGenre, Image image)
        {
            string dirPath = curDirectoryProject_images;
            string filePath = Path.Combine(curDirectoryProject_images, tb_FilmName.Text + ".jpg");
            string imagePath = filePath;
            BitmapImage bitmap_default = new BitmapImage();
            bitmap_default.BeginInit();
            bitmap_default.CacheOption = BitmapCacheOption.OnLoad;
            bitmap_default.UriSource = new Uri(File.Exists(filePath) ? filePath : Path.Combine(Add_Film.curDirectoryProject_main, "default_img.jpg"));
            bitmap_default.EndInit();
            bitmap_default.Freeze();

            //BitmapImage bitmap_default = new BitmapImage(File.Exists(imagePath) ? new Uri(imagePath) : new Uri(Path.Combine(Add_Film.curDirectoryProject_main, "default_img.jpg")));



            //загрузка картинки в отведенное окно
            img_drop.Source = bitmap_default;

            
            using (var con = new SQLiteConnection(dB_Class.db_path))
            {
                con.Open();
                using (var cmd = new SQLiteCommand(con))
                {
                    cmd.CommandText = @"SELECT * FROM Films WHERE title='" + tb_FilmName.Text + "'";

                    SQLiteDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        //отображение имени фильма
                        tb_name.Text = reader.GetString(1);
                        //отображение года фильма
                        tb_year.Text = reader.GetInt32(2).ToString();
                        tb_img_path.Text = filePath;


                        //отображение жанров в listBox
                        #region Заполнение listBox данными с UC
                        string ganreText = tb_FilmGenre.Text;
                        string[] ganres = ganreText.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

                        var generator = listBox_ganre.ItemContainerGenerator;
                        generator.StatusChanged += (s, e) =>
                        {
                            if (generator.Status == GeneratorStatus.ContainersGenerated)
                            {
                                foreach (var item in listBox_ganre.Items)
                                {
                                    ListBoxItem listBoxItem = generator.ContainerFromItem(item) as ListBoxItem;

                                    if (listBoxItem != null)
                                    {
                                        listBoxItem?.Focus();
                                        string genre = listBoxItem.Content.ToString();

                                        if (ganres.Contains(genre))
                                        {
                                            listBoxItem.IsSelected = true;
                                        }
                                        else
                                        {
                                            listBoxItem.IsSelected = false;
                                        }
                                    }
                                }
                            }
                        };
                        #endregion
                        btn_addFinish.Visibility = Visibility.Hidden;
                        btn_updateFinish.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        //Заполнение данных о жанрах в listBox
        private void listBox_ganre_allFromDB()
        {
            using (var con = new SQLiteConnection(dB_Class.db_path))
            {
                con.Open();
                using (var cmd = new SQLiteCommand(con))
                {
                    cmd.CommandText = @"SELECT * FROM Genres";

                    SQLiteDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        listBox_ganre.Items.Add(reader.GetString(1));
                    }
                    reader.Close();
                    cmd.ExecuteNonQuery();
                }
                con.Close();
            }
        }


        private void btn_addFinish_Click(object sender, RoutedEventArgs e)
        {
            var maxYear = DateTime.Today.AddYears(20);
            string inputFormat = "yyyy";

            #region Проверки данных
            if (!DateTime.TryParseExact(tb_year.Text, inputFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime year))
            {
                MessageBox.Show("Некорректный формат года!", "Некорректный год", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else if (year > maxYear)
            {
                MessageBox.Show($"Год не может быть позже {maxYear.Year} года!", "Некорректный год", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else if (listBox_ganre.SelectedItems.Count==0)
            {
                MessageBox.Show("Вы не выбрали жанр.", "Некорректный жанр", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else if (string.IsNullOrWhiteSpace(tb_name.Text))
            {
                MessageBox.Show("Вы не ввели название фильма.", "Отсутсвует название", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            #endregion
            else
            {
                try
                {
                    dB_Class.add_film(tb_name, tb_year, listBox_ganre);

                    #region Сохранение изображение (+ сжатие)
                    // получаем BitmapSource из Image
                    BitmapSource bitmapSource = (BitmapSource)img_drop.Source;

                    string imageFile = curDirectoryProject_images + "\\" + tb_name.Text + ".jpg";

                    // создаем JpegBitmapEncoder и вызываем его метод Encode      
                    JpegBitmapEncoder encoder = new JpegBitmapEncoder();

                    // сжатие изображение, если оно больше 500х500
                    if (bitmapSource.Width > 500 && bitmapSource.Height > 500)
                        encoder.QualityLevel = 50; // устанавливаем низкое качество

                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                    // создаем FileStream для записи в файл
                    using (FileStream file = new FileStream(imageFile, FileMode.Create))
                    {
                        encoder.Save(file);
                    }
                    #endregion

                    MessageBox.Show("Данные добавлены.", "Успешно", MessageBoxButton.OK, MessageBoxImage.None);

                }
                catch (SQLiteException ex) when (ex.ResultCode == SQLiteErrorCode.Constraint)
                {
                    MessageBox.Show("Такое имя уже есть!", "Некорректное имя", MessageBoxButton.OK, MessageBoxImage.Warning);
                }


            }
        }

        #region Загрузка изображения с проводника (перетаскивание/путь)
        private void img_drop_PreviewDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                var bitmap = new BitmapImage(new Uri(files[0]));
                img_drop.Source = bitmap;
            }
        }

        private void img_drop_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;
        }

        private void btn_img_path_Click(object sender, RoutedEventArgs e)
        {

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFilePath = openFileDialog.FileName;
                tb_img_path.Text = selectedFilePath;
                //отображение по пути, который получили
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(selectedFilePath);
                bitmap.EndInit();
                img_drop.Source = bitmap;

            }
        }
        #endregion


        #region Вставка изображения через CTRL+V
        private void PasteImageFromClipboard()
        {
            if (Clipboard.ContainsImage())
            {
                var bitmap = Clipboard.GetImage() as BitmapSource;
                img_drop.Source = bitmap;
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
            {
                PasteImageFromClipboard();
            }

        }
        #endregion

        private void btn_updateFinish_Click(object sender, RoutedEventArgs e)
        {
            dB_Class.update_film(tb_name, tb_year, listBox_ganre);
        }
    }
}
