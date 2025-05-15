using System;
using System.Data.SQLite;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using WpfApp1.UserControls;

namespace WpfApp1.DB
{
    class DB_class
    {
        //путь до папки проекта на 2 папки выше debug ()
        private static string path_to_directory = Directory.GetParent(Directory.GetParent(Environment.CurrentDirectory).FullName).FullName;

        public string db_path { get; }

        public DB_class()
        {
            db_path = @"URI=file:" + path_to_directory + @"\MyDatabase10.sqlite";
        }


    public void create_and_open()
        {
            using (var con = new SQLiteConnection(db_path))
            {
                con.Open();
                using (var cmd = new SQLiteCommand(con))
                {
                    // Note New: This (SQL) would be good to create in DB but not in code
                    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Films (
                        id INTEGER PRIMARY KEY NOT NULL UNIQUE,
                        title TEXT NOT NULL UNIQUE,
                        release_year INTEGER,
                        image TEXT UNIQUE
                    );

                    CREATE TABLE IF NOT EXISTS Genres (
                        id INTEGER PRIMARY KEY NOT NULL UNIQUE,
                        name TEXT UNIQUE
                    );

                    CREATE TABLE IF NOT EXISTS FilmGenres (
                        film_id INTEGER,
                        genre_id INTEGER,
                        FOREIGN KEY (film_id)
                            REFERENCES Films (id)
                                ON UPDATE CASCADE
                                ON DELETE CASCADE,
                        FOREIGN KEY (genre_id)
                            REFERENCES Genres (id)
                                ON UPDATE CASCADE
                                ON DELETE CASCADE,
                        PRIMARY KEY (film_id, genre_id)
                    );
                    ";
                    cmd.ExecuteNonQuery();


                    cmd.CommandText = @"INSERT OR IGNORE INTO Genres (name) VALUES 
                                        ('аниме'),
                                        ('биография'),
                                        ('боевик'),
                                        ('вестерн'),
                                        ('военный'),
                                        ('детектив'),
                                        ('детский'),
                                        ('документальный'),
                                        ('драма'),
                                        ('исторический'),
                                        ('кинокомикс'),
                                        ('комедия'),
                                        ('концерт'),
                                        ('короткометражный'),
                                        ('криминал'),
                                        ('мелодрама'),
                                        ('мистика'),
                                        ('музыка'),
                                        ('мультфильм'),
                                        ('мюзикл'),
                                        ('научный'),
                                        ('нуар'),
                                        ('приключения'),
                                        ('реалити-шоу'),
                                        ('семейный'),
                                        ('спорт'),
                                        ('ток-шоу'),
                                        ('триллер'),
                                        ('ужасы'),
                                        ('фантастика'),
                                        ('фэнтези')";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = @"CREATE TRIGGER IF NOT EXISTS my_trigger 
                                        BEFORE INSERT ON Films
                                                FOR EACH ROW
                                                BEGIN
                                       SELECT 
                                            COUNT(*) AS count
                                        FROM 
                                            Films 
                                        WHERE 
                                            CASE 
                                                WHEN EXISTS(SELECT 1 FROM Films WHERE title = NEW.title) THEN RAISE(ABORT, 'Такое имя уже существует!') 
                                                ELSE 1 
                                            END = 1;			
                                        END;
                                        ";
                    cmd.ExecuteNonQuery();

                }
                con.Close();
            }
        }

        #region Clear table (-s)
        public void clear_films_table()
        {
            using (var con = new SQLiteConnection(db_path))
            {
                con.Open();
                using (var cmd = new SQLiteCommand(con))
                {
                    cmd.CommandText = @"
                                        DROP TABLE Films;
                                      ";
                    cmd.ExecuteNonQuery();
                }
                con.Close();
            }
        }
        public void clear_all_tables()
        {
            using (var con = new SQLiteConnection(db_path))
            {
                con.Open();
                using (var cmd = new SQLiteCommand(con))
                {
                    cmd.CommandText = @"
                                        DROP TABLE Films;
                                        DROP TABLE Genres;
                                        DROP TABLE FilmGenres;
                                      ";
                    cmd.ExecuteNonQuery();
                }
                    con.Close();
                }
            }
        #endregion




        public void update_film(TextBox tb_title, TextBox tb_year, ListBox listBox_film)
        {
            using (var con = new SQLiteConnection(db_path))
            {
                con.Open();
                using (var cmd = new SQLiteCommand(con))
                {
                    string str_new = Regex.Replace(tb_title.Text, @"\s+", " ");
                    str_new = str_new.Trim();

                    cmd.CommandText = @"UPDATE Films SET title = @title, release_year = @year, image = @image_name WHERE title = '" + str_new + "';";

                    //удалить пробелы, если больше одного

                    cmd.Parameters.AddWithValue("@title", str_new);
                    cmd.Parameters.AddWithValue("@year", tb_year.Text);
                    cmd.Parameters.AddWithValue("@image_name", str_new);
                    cmd.ExecuteNonQuery();

                    int update_film_id = 0;
                    cmd.CommandText = @"SELECT id FROM Films WHERE title='"+ str_new + "'";
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        update_film_id = Convert.ToInt32(result);
                    }

                    cmd.CommandText = @" DELETE FROM Films WHERE film_id='"+ update_film_id + "'";
                    cmd.ExecuteNonQuery();

                    foreach (object selectedItem in listBox_film.SelectedItems)
                    {
                        int index = listBox_film.Items.IndexOf(selectedItem);
                        cmd.CommandText = @"INSERT OR IGNORE INTO FilmGenres (film_id, genre_id) VALUES (" + update_film_id + ", " + (index + 1) + ")";
                        cmd.ExecuteNonQuery();
                    }
                }
                con.Close();

            }
        }

        public void add_film(TextBox tb_title, TextBox tb_year, ListBox listBox_film)
        {
            using (var con = new SQLiteConnection(db_path))
            {
                con.Open();
                using (var cmd = new SQLiteCommand(con))
                {
                    cmd.CommandText = @"INSERT OR IGNORE INTO Films (title, release_year, image) VALUES (@title, @year, @image_name)";

                    //удалить пробелы, если больше одного
                    string str_new = Regex.Replace(tb_title.Text, @"\s+", " ");
                    str_new = str_new.Trim();

                    cmd.Parameters.AddWithValue("@title", str_new);
                    cmd.Parameters.AddWithValue("@year", tb_year.Text);
                    cmd.Parameters.AddWithValue("@image_name", str_new);
                    cmd.ExecuteNonQuery();

                    int last_inserted_id_film = 0;
                    cmd.CommandText = @"SELECT last_insert_rowid() FROM Films";
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        last_inserted_id_film = Convert.ToInt32(result);
                    }

                    foreach (object selectedItem in listBox_film.SelectedItems)
                    {
                        int index = listBox_film.Items.IndexOf(selectedItem);
                        cmd.CommandText = @"INSERT OR IGNORE INTO FilmGenres (film_id, genre_id) VALUES (" + last_inserted_id_film + ", " + (index+1) + ")";
                        cmd.ExecuteNonQuery();
                    }
                }
                con.Close();

            }
        }
    }
}
