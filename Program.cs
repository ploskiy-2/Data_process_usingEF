using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.IO;
using System.Text.RegularExpressions;
using System.Net.NetworkInformation;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Formats.Tar;


namespace DataProcForWebApp
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Movie> Movies => Set<Movie>();
        public DbSet<Human> Humans => Set<Human>();

        public DbSet<Tag> Tags => Set<Tag>();

        public ApplicationContext() => Database.EnsureCreated();


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=helloapp.db");
            optionsBuilder.EnableSensitiveDataLogging();

        }
    }

    public class Movie
    {
        [Key]
        public int id { get; set; }
        public string tittle { get; set; }
        public Movie(string current_tittle)
        {
            tittle = current_tittle;
        }

        public Movie()
        {
        }

        [NotMapped]
        public HashSet<string> actorsSet = new HashSet<string>();

        //потенциально похожие фильмы
        [NotMapped]
        public HashSet<string> potentialSimilaryMovie = new HashSet<string>();

        public List<Human> actorsSetConnection { get; set; } = new List<Human>();
        public List<Tag> tagssSetConnection { get; set; } = new List<Tag>();
        public string? director { get; set; }

        [NotMapped]
        public HashSet<string> tagSet = new HashSet<string>();
        public string? movieRating { get; set; }
    }

    public class Human
    {
        [Key]
        public int id { get; set; }
        public string name { get; set; }
        public Human(string cur_name)
        {
            name = cur_name;
        }
        public Human()
        {
        }
        public HashSet<Movie> currentMoviesConnection { get; set; } = new HashSet<Movie>();
    }


    public class Tag
    {
        [Key]
        public int id { get; set; }
        public string tittle { get; set; }
        public Tag(string cur_tittle)
        {
            tittle = cur_tittle;
        }
        public Tag()
        {
        }
        public HashSet<Movie> currentMoviesConnection { get; set; } = new HashSet<Movie>();
    }

    /// This class will contain methods for processing web application data at different stages
    public static class PipelineStages
    {
        ///filename - the path to the directory from where to extract the movie code
        /// output - the dictionary where the file names will be written with them code
        public static async Task RecieveMovieImdbCodesAsync(string filename, ConcurrentDictionary<string, string> output, ConcurrentDictionary<string, Movie> allMovies)
        {
            using (FileStream stream = File.OpenRead(filename))
            using (var reader = new StreamReader(stream))
            {
                string line;
                var tasks = new List<Task>();
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    int tab1 = line.IndexOf('\t');
                    int tab2 = line.IndexOf('\t', tab1 + 1);
                    int tab3 = line.IndexOf('\t', tab2 + 1);
                    int tab4 = line.IndexOf('\t', tab3 + 1);
                    int tab5 = line.IndexOf('\t', tab4 + 1);

                    string country1 = line.Substring(tab3 + 1, tab4 - tab3 - 1);
                    string country2 = line.Substring(tab4 + 1, tab5 - tab4 - 1);
                    if ((country1 == "RU" || country1 == "EN" || country1 == "US" || country1 == "GB") || (country2 == "RU" || country2 == "EN" || country2 == "US" || country2 == "GB"))
                    {
                        string movieCode = line.Substring(0, tab1);
                        string movieTitle = line.Substring(tab2 + 1, tab3 - tab2 - 1);
                        Movie movie = new Movie(movieTitle);
                        allMovies.AddOrUpdate(movieTitle, movie, (existingKey, existingValue) => existingValue);
                        output.AddOrUpdate(movieCode, movieTitle, (existingKey, existingValue) => existingValue);
                    }
                }
            }
        }

        // сопостовляем кодам на imdb коды на Lens, создавая словарь где ключ код на имдб а значение это объект класса Movie
        public static Task RecieveMovieLensCodesAsync(string filename, ConcurrentDictionary<string, string> output, ConcurrentDictionary<string, string> filmsCodeIMDB, ConcurrentDictionary<string, Movie> allMovies)
        {

            return Task.Factory.StartNew(() =>
            {
                using (FileStream stream = File.OpenRead(filename))
                {
                    var reader = new StreamReader(stream);
                    string line = null;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] columns = line.Split(',');
                        string movieIMDBCode = "tt" + columns[1]; //код на имдб
                        string movieLensCode = columns[0]; //код на lens

                        //int tab1 = line.IndexOf(',');
                        //int tab2 = line.IndexOf(',', tab1 + 1);
                        //string movieIMDBCode = line.Substring(tab1 + 1, tab2 - tab1 - 1);
                        //string movieLensCode = line.Substring(0,tab1);
                        //по коду на имдб получаем название фильма из словаря filmsCodeIMDB
                        bool flagForTittleForMovie = filmsCodeIMDB.TryGetValue(movieIMDBCode, out string movieTittle);
                        if (flagForTittleForMovie)
                        {
                            //по названию фильма получаем объект класса Movie - currentMovie. от которого хотим получить его название( поле tittle) 
                            bool flagForCurrentForMovie = allMovies.TryGetValue(movieTittle, out Movie currentMovie);
                            if (flagForCurrentForMovie) { output.AddOrUpdate(movieLensCode, currentMovie.tittle, (existingKey, existingValue) => existingValue); }
                        }
                    }
                }
            }, TaskCreationOptions.LongRunning);/// report that this is long running task
        }

        //получаем по кодам людей их имя
        public static Task ReceiveActorsAndDirectorsNamesAsync(string filename, ConcurrentDictionary<string, string> output)
        {
            return Task.Factory.StartNew(() =>
            {
                using (FileStream stream = File.OpenRead(filename))
                {
                    var reader = new StreamReader(stream);
                    string line = null;
                    while ((line = reader.ReadLine()) != null)
                    {
                        int tab1 = line.IndexOf('\t');
                        int tab2 = line.IndexOf('\t', tab1 + 1);

                        string codActore = line.Substring(0, tab1);
                        string nameActor = line.Substring(tab1 + 1, tab2 - tab1 - 1);
                        { output.AddOrUpdate(codActore, nameActor, (existingKey, existingValue) => existingValue); }
                    }
                }
            }, TaskCreationOptions.LongRunning);/// report that this is long running task

        }



        // получаем по коду данного человека множество фильмов где он принял участие
        public static Task ReceiveActorsAndDirectorsCodesAsync(string filename, ConcurrentDictionary<string, HashSet<Movie>> output, ConcurrentDictionary<string, string> dictHumansCode, ConcurrentDictionary<string, Movie> allMovies, ConcurrentDictionary<string, string> dictMovieCodes)
        {
            return Task.Factory.StartNew(() =>
            {
                using (FileStream stream = File.OpenRead(filename))
                {
                    //the same beginning as in the previous tasks
                    var reader = new StreamReader(stream);
                    string line = null;
                    while ((line = reader.ReadLine()) != null)
                    {
                        //string[] columns = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        int tab1 = line.IndexOf('\t');
                        int tab2 = line.IndexOf('\t', tab1 + 1);
                        int tab3 = line.IndexOf('\t', tab2 + 1);
                        int tab4 = line.IndexOf('\t', tab3 + 1);

                        string categoryActors = line.Substring(tab3 + 1, tab4 - tab3 - 1);

                        if (categoryActors == "director" || categoryActors == "actor") //если данный человек актер или режиссер
                        {
                            string movieCode = line.Substring(0, tab1); //получаем код данного фильма
                            string humanCode = line.Substring(tab2 + 1, tab3 - tab2 - 1); // получаем код этого человека
                            //по коду человека в словаре сопоставляющему коды людей и их имена, получаем имя человека
                            bool flagForHuman = dictHumansCode.TryGetValue(humanCode, out string humansName);
                            //по коду фильма в словаре сопоставляющему коды фильма и их названия, получаем название фильма
                            bool flagForTittleForMovie = dictMovieCodes.TryGetValue(movieCode, out string movieTittle);
                            if (flagForHuman && flagForTittleForMovie) //если все нашлось то
                            {
                                //в словаре всех фильмов, по названию фильма(ключ) получаем объект класса movie, поля
                                //которого мы будем далее менять
                                bool flagForMovie = allMovies.TryGetValue(movieTittle, out Movie currentMovie);
                                if (categoryActors == "director") { currentMovie.director = humansName; }//пытаемся добавить режиссера
                                if (categoryActors == "actor")
                                {
                                    currentMovie.actorsSet.Add(humansName);
                                }//в список актеров фильма добавляем данного актера
                                //в словарь где ключом является имя человека, а значение это множество фильмо где он принял участие
                                // пытаемся создать такую пару ключ-значение
                                // если такая пара уже есть, то к множеству фильмов надо добавить текущий фильм
                                output.AddOrUpdate(humansName, new HashSet<Movie>() { currentMovie }, (existingKey, existingValue) =>
                                {
                                    existingValue.Add(currentMovie);
                                    return existingValue;
                                });
                            }
                        }
                    }
                }
            }, TaskCreationOptions.LongRunning);/// report that this is long running task

        }


        // получаем по коду фильма его рейтинг на имдб
        public static Task ReceiveMovieRatingAsync(string filename, ConcurrentDictionary<string, Movie> allMovies, ConcurrentDictionary<string, string> dictMovieCodes)
        {
            return Task.Factory.StartNew(() =>
            {
                using (FileStream stream = File.OpenRead(filename))
                {
                    //the same beginning as in the previous tasks
                    var reader = new StreamReader(stream);
                    string line = null;
                    while ((line = reader.ReadLine()) != null)
                    {
                        //string[] columns = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        int tab1 = line.IndexOf('\t');
                        int tab2 = line.IndexOf('\t', tab1 + 1);
                        string movieCode = line.Substring(0, tab1); //получаем код данного фильма
                        string currentMovieRating = line.Substring(tab1 + 1, tab2 - tab1 - 1); // получаем рейтинг данного фильма
                        //по коду фильма в словаре сопоставляющему коды фильма и их названия, получаем название фильма
                        bool flagForTittleForMovie = dictMovieCodes.TryGetValue(movieCode, out string movieTittle);
                        if (flagForTittleForMovie) //если все нашлось то
                        {
                            //в словаре всех фильмов, по названию фильма(ключ) получаем объект класса movie, поля
                            //которого мы будем далее менять
                            bool flagForMovie = allMovies.TryGetValue(movieTittle, out Movie currentMovie);
                            currentMovie.movieRating = currentMovieRating;//в список актеров фильма добавляем данного актера
                        }
                    }
                }
            }, TaskCreationOptions.LongRunning);/// report that this is long running task

        }



        //получаем по айди тега его название
        public static Task ReceiveTagsIdAsync(string filename, ConcurrentDictionary<string, string> output)
        {
            return Task.Factory.StartNew(() =>
            {
                using (FileStream stream = File.OpenRead(filename))
                {
                    var reader = new StreamReader(stream);
                    string line = null;

                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] columns = line.Split(',');

                        string tagsId = columns[0]; //код тега
                        string tagsName = columns[1]; //именование тега
                        output.AddOrUpdate(tagsId, tagsName, (existingKey, existingValue) => existingValue);
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }

        //создание словаря тег - множество фильмов данного тега 
        //dictionaryTagsId - словарь где ключ это ид тега а значение сам тег
        // output - это словарь где коюч тег а значение множество его фильмов
        //allmovies - все фильмы 
        //LensMovies - ключ код на ленс а значение название фильма
        public static Task ReceiveTagsMoviesAsync(string filename, ConcurrentDictionary<string, string> dictionaryTagsId, ConcurrentDictionary<string, HashSet<Movie>> output, ConcurrentDictionary<string, Movie> allMovies, ConcurrentDictionary<string, string> LensMovies)
        {
            return Task.Factory.StartNew(() =>
            {
                using (FileStream stream = File.OpenRead(filename))
                {
                    var reader = new StreamReader(stream);
                    string line = null;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] columns = line.Split(',');
                        int tab1 = line.IndexOf(',');
                        int tab2 = line.IndexOf(',', tab1 + 1);

                        string movieId = line.Substring(0, tab1); //код фильма на ленс
                        string tagsId = line.Substring(tab1 + 1, tab2 - tab1 - 1); //id тега
                        string relevance = line.Substring(tab2 + 1).Replace('.', ','); //получаем соответствие тега и фильма

                        if (double.TryParse(relevance, out double number))
                        {
                            if (number > 0.6)
                            {
                                //из словаря где ключ это id на lens получаем название фильма
                                bool flagForTittleForMovie = LensMovies.TryGetValue(movieId, out string movieTittle);
                                if (flagForTittleForMovie)
                                {

                                    // по названию фильма получаем объект класса movie
                                    bool flagForCurrentMovie = allMovies.TryGetValue(movieTittle, out Movie currentMovie);
                                    if (flagForCurrentMovie)
                                    {
                                        //по айди тегу получаем название тега и доавбляем его в множество текущего фильма, а так же в словарь
                                        // тег - множество фильмов 
                                        bool flagForNameTag = dictionaryTagsId.TryGetValue(tagsId, out string nameTag);
                                        currentMovie.tagSet.Add(nameTag);
                                        output.AddOrUpdate(nameTag, new HashSet<Movie>() { currentMovie }, (existingKey, existingValue) =>
                                        {
                                            existingValue.Add(currentMovie);
                                            return existingValue;
                                        });
                                    }
                                }

                            }
                        }
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }

    }
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //await Parsing();
            while (true)
            {
                Console.WriteLine("Выберите нужный вариант для Вас");
                Console.WriteLine("a - распечатать инфмормацию о фильме");
                Console.WriteLine("b - распечатать информация об актере");
                Console.WriteLine("c - распечатать информацию о теге");
                Console.WriteLine("если хотите выйте - напишите exit");
                string variantChar = Console.ReadLine();
                if (variantChar == "exit") { break; }
                if (variantChar == "a")
                {
                    using (ApplicationContext db = new ApplicationContext())
                    {

                        Console.Clear();
                        Console.WriteLine("Введите название фильма:");
                        string tittleMovie = Console.ReadLine();
                        Console.WriteLine();

                        var movie = db.Movies.FirstOrDefault(m => m.tittle == tittleMovie);
                        if (movie != null)
                        {
                            var actorNames = from mov in db.Movies
                                             where mov.tittle == tittleMovie
                                             from ac in mov.actorsSetConnection
                                             select ac;
                            var tagTittles = from mov in db.Movies
                                                 where mov.tittle == tittleMovie
                                                 from tag in mov.tagssSetConnection
                                                 select tag;
                            Console.WriteLine("Название этого фильма:" + " " + movie.tittle);
                            Console.WriteLine("Рейтинг этого фильма:" + " " + movie.movieRating);
                            Console.WriteLine("Режиссер этого фильма:" + " " + movie.director);

                            string moviesactorNames = "";
                            foreach (var t in actorNames)
                            {
                                moviesactorNames += t.name + " ";
                            }

                            Console.WriteLine("Актеры фильма" + " " + moviesactorNames);
                            
                            string moviesWithTags = "";
                            foreach (var t in tagTittles)
                            {
                                moviesWithTags += t.tittle + " ";
                            }
                            Console.WriteLine("Теги фильма" + " " + moviesWithTags);
                            

                            Console.WriteLine();
                            Console.WriteLine("10 похожих фильмов: ");
                            /*Dictionary<string, double> similaryMovies = GetSimilaryMovies(movie,actorNames,tagTittles, db);
                            foreach (var t in similaryMovies)
                            {
                               // Console.WriteLine(string.Join(", ", t.Key + " " + t.Value + " " + "схожести"));
                            }*/
                        }
                        else { Console.WriteLine("Этого фильма нет в базе данных/Этот фильм не на русском/английском"); }
                    }
                }
                if (variantChar == "b")
                {
                    using (ApplicationContext db = new ApplicationContext())
                    {
                        Console.Clear();
                        Console.WriteLine("Введите имя актера:");
                        string humanName = Console.ReadLine();
                        Console.WriteLine();
                        /*
                        var human = from human in db.Humans
                                           where human.name == humanName
                                           from movie in human.currentMoviesConnection
                                           select movie.tittle;*/
                        var hum = db.Humans.FirstOrDefault(p => p.name==humanName);
                        if (hum!=null)
                        {
                            
                            Console.WriteLine("Фильмы, в которых он/она принял/а участие:" + " " + string.Join(", ", 
                                from t in hum.currentMoviesConnection select t.tittle));
                        }
                        else
                        {
                            Console.WriteLine("Этого человека нет в базе данных");
                        }
                    }

                }
                if (variantChar == "c")
                {
                    using (ApplicationContext db = new ApplicationContext())
                    {

                        Console.Clear();
                        Console.WriteLine("Введите название тега:");
                        string tagName = Console.ReadLine();
                        Console.WriteLine();
                        var moviesTitlesTags = from tag in db.Tags
                                               where tag.tittle == tagName
                                               from movie in tag.currentMoviesConnection
                                               select movie.tittle;

                        if (moviesTitlesTags.Count() > 0)
                        {
                            Console.WriteLine("Фильмы, помеченные данным тегом:" + " " + string.Join(", ", moviesTitlesTags));
                        }
                        else { Console.WriteLine("Этого тега нет в базе данных"); }
                    }
                    Console.WriteLine();
                }
            }
        }

        static Dictionary<string, double> GetSimilaryMovies(Movie currentMovie, IEnumerable<Human> actorMovies, IEnumerable<Tag> tagMovies, ApplicationContext db)
        {
        //идея для сравнения фильмов: давайте если у фильмов одинаковые режиссеры то добавляем к рейтингу схожести 0,05
        //если одинаковый актер то +0,025, если одинаковый тег, то +0,01, затем в конце выбрать min{0.5, сумма добавок}
            Dictionary<string, double> rankedSimilaryMovie = new Dictionary<string, double>();
            foreach (var actor in actorMovies)
            {
                //получаем список фильмов данного актера (который принял участие в этом фильме
                var t = db.Humans.Where(t => t.name == actor.name);
                foreach (var a in t) 
                {
                    currentMovie.potentialSimilaryMovie.UnionWith(a.currentMoviesConnection.Select(m => m.tittle));
                    Console.WriteLine(a.currentMoviesConnection.Count());
                }
                //Console.WriteLine(t.First().name + " " + t.Count());
            };
            //Console.WriteLine(currentMovie.potentialSimilaryMovie.Count);
            foreach (var tag in tagMovies)
            {
                //получаем список фильмов данного тега 
                foreach (var a in db.Tags.Where(t => t.tittle == tag.tittle))
                {
                    currentMovie.potentialSimilaryMovie.UnionWith(a.currentMoviesConnection.Select(m => m.tittle));
                }
            };
            currentMovie.potentialSimilaryMovie.Remove(currentMovie.tittle);
            foreach (var simMovie in currentMovie.potentialSimilaryMovie)
            {
                double rank = 0;
                var compareMovie = db.Movies.FirstOrDefault(m => m.tittle == simMovie);
                HashSet<string> commonActorMovie = new HashSet<string>(compareMovie.actorsSet);
                HashSet<string> commonTagMovie = new HashSet<string>(compareMovie.tagSet);

                commonActorMovie.Intersect(currentMovie.actorsSet);
                commonTagMovie.Intersect(currentMovie.tagSet);

                var t = commonActorMovie.Count;
                var tt = commonTagMovie.Count;

                if ((compareMovie.director == currentMovie.director))
                {
                    rank = 0.05;
                }
                rank = Math.Min(rank + t * 0.025 + tt * 0.01, 0.5);
                rankedSimilaryMovie.Add(simMovie, rank);
                //Console.WriteLine(simMovie + " ---- " + rank);
            };

            var sortedDictMovies = from mov in rankedSimilaryMovie orderby mov.Value descending select mov;
            return sortedDictMovies.Take(10).ToDictionary();
            
        }
        static async Task Parsing()
        {
            {
                ///I decided not to create a directory of the names of all the files,
                ///since they all have different information and I need to distinguish them somehow

                ///Dictionary of all movies with a key - name, value - object of the Movie class
                var allMoviesImdb = new ConcurrentDictionary<string, Movie>();
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();



                ///filename  - the path that contains movies with codes on imdb.
                ///filmsCodeIMDB_RU_EN - dictionary containing the names and codes of 
                ///films in Russian or English (or with such a region)
                string filename_movieCodeImdb = @"C:\Users\VLADIMIR\Desktop\ml-latest\ml-latest\MovieCodes_IMDB.tsv";
                string filename_movieCodeLens = @"C:\Users\VLADIMIR\Desktop\ml-latest\ml-latest\links_IMDB_MovieLens.csv";
                //ключ  - это код фильма на сайте, значение это название фильма
                var filmsCodeIMDB_RU_EN = new ConcurrentDictionary<string, string>();
                var filmsCodeLens_RU_EN = new ConcurrentDictionary<string, string>();
                ///task which creates filmsCodeIMDB_RU_EN dictionary
                Task createDictionaryFilmsImdbCode = PipelineStages.RecieveMovieImdbCodesAsync(filename_movieCodeImdb, filmsCodeIMDB_RU_EN, allMoviesImdb);


                await createDictionaryFilmsImdbCode;
                Task createDictionaryFilmsLenCode = PipelineStages.RecieveMovieLensCodesAsync(filename_movieCodeLens, filmsCodeLens_RU_EN, filmsCodeIMDB_RU_EN, allMoviesImdb);




                /// we create a dictionary along this path with 
                /// the name of the actor / director and his code
                string filename_actors_directorsNames = @"C:\Users\VLADIMIR\Desktop\ml-latest\ml-latest\ActorsDirectorsNames_IMDB.txt";
                var actorsDirectorsNames = new ConcurrentDictionary<string, string>();
                ///the task that creates the dictionary
                Task createDictionaryActorsDirectorsNames = PipelineStages.ReceiveActorsAndDirectorsNamesAsync(filename_actors_directorsNames, actorsDirectorsNames);


                /// along this path, the codes of all the films in which 
                /// this actor starred (referring to the actor through the name)
                string filename_actors_directorsCodes = @"C:\Users\VLADIMIR\Desktop\ml-latest\ml-latest\ActorsDirectorsCodes_IMDB.tsv";
                /// dictionary where the key is the name of the actor / director, 
                /// the value is the set of films where he took part
                var finallyActorsDirectorsDict = new ConcurrentDictionary<string, HashSet<Movie>>();
                Task createDictionaryActorsDirectorsCodes = PipelineStages.ReceiveActorsAndDirectorsCodesAsync(filename_actors_directorsCodes, finallyActorsDirectorsDict, actorsDirectorsNames, allMoviesImdb, filmsCodeIMDB_RU_EN);




                //the path where the movie ratings file is located
                string filename_moviesRating = @"C:\Users\VLADIMIR\Desktop\ml-latest\ml-latest\Ratings_IMDB.tsv";
                // получаем для каждого фильма его рейтинг на имдб
                Task createDictionaryMovieRating = PipelineStages.ReceiveMovieRatingAsync(filename_moviesRating, allMoviesImdb, filmsCodeIMDB_RU_EN);

                await Task.WhenAll(createDictionaryActorsDirectorsCodes, createDictionaryActorsDirectorsNames, createDictionaryMovieRating, createDictionaryFilmsLenCode);


                string filename_TagsLenCodes = @"C:\Users\VLADIMIR\Desktop\ml-latest\ml-latest\TagCodes_MovieLens.csv";
                string filename_TagsLenScores = @"C:\Users\VLADIMIR\Desktop\ml-latest\ml-latest\TagScores_MovieLens.csv";

                //словарь где ключ это название тега, а значение это множество фильмов с таким тегом 
                var finallyTagsDictionary = new ConcurrentDictionary<string, HashSet<Movie>>();

                //
                //По id тега я получаю сам тег
                var dictionaryTagsId = new ConcurrentDictionary<string, string>();
                Task createDictionaryTgsId = PipelineStages.ReceiveTagsIdAsync(filename_TagsLenCodes, dictionaryTagsId);


                //передаем два файла, из файла filename_TagsLenCodes мы получаем код фильма на lens (первая колонка)
                // по второй колонке ищем по id тэга его название 
                //если в файле filename_TagsLenScores соотстветвие более 0,5 то добавляем тэг в множестве в классе Movie , и в выходной словарь данного тэга
                Task createDictionaryTagsDictionary = PipelineStages.ReceiveTagsMoviesAsync(filename_TagsLenScores, dictionaryTagsId, finallyTagsDictionary, allMoviesImdb, filmsCodeLens_RU_EN);


                await createDictionaryTgsId;
                await createDictionaryTagsDictionary;
                stopWatch.Stop();
                // Get the elapsed time as a TimeSpan value.
                TimeSpan ts = stopWatch.Elapsed;

                // Format and display the TimeSpan value.
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds,
                    ts.Milliseconds / 10);
                Console.WriteLine("RunTime " + elapsedTime);
                

          
                using (ApplicationContext db = new ApplicationContext())
                {                   
                    await db.Movies.AddRangeAsync(allMoviesImdb.Values);
                    foreach (var h in finallyActorsDirectorsDict)
                    {
                        if (h.Key != null)
                        {                            
                            Human human = new Human(h.Key);
                            db.Humans.Add(human);
                            //addHumanRecordYet.Add(human);
                            
                            foreach (var m in h.Value)
                            {
                                m.actorsSetConnection.Add(human);
                                human.currentMoviesConnection.Add(m);
                            }                        
                        }
                    };

                    foreach (var t in finallyTagsDictionary)
                    {
                        if (t.Key != null)
                        {
                            Tag tag =  new Tag(t.Key);
                            db.Tags.Add(tag);
                           // addTagRecordYet.Add(tag);
                            
                            foreach (var m in t.Value)
                            {
                                m.tagssSetConnection.Add(tag);
                                tag.currentMoviesConnection.Add(m);
                            }
                        }
                    };
                  
                    await db.SaveChangesAsync();
                }              
            }
        }
    }
}

