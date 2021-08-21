using System;
using System.Collections.Generic;
using System.Linq;
using ivy_league.Models;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace ivy_league
{
    class Program
    {
        private static Game _game;

        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("./IvyLeagueLog.log")
                .CreateLogger();

            Log.Information("test");
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            var students = CreateStudents();
            var buildings = CreateBuildings();
            var players = CreatePlayers();

            _game = new Game(students, buildings, players);
            Play();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(configure => configure.AddSerilog())
                .AddTransient<Game>();
        }

        public static void Play()
        {
            Log.Information("Play Game");
            Setup();

            //while (_game.StudentsDeck.Count != 0)
            //{
                foreach (var player in _game.Players)
                {
                    TakeTurn(player);
                };
            //}
        }

        private static void TakeTurn(Player player)
        {
            //TODO:// fill out turn production on player

            SetTurnProduction(player);
            PickCard(player);
        }

        private static void Setup()
        {
            _game.BuildingsDeck.Shuffle<Building>();
            _game.StudentsDeck.Shuffle<Student>();

            FlipNewStudentCards();
            FlipNewBuildingCards();
            GiveRandomChancellorToEachPlayer();

            _game.Players.ForEach(x => x.Coins = 3);
        }

        private static void FlipNewStudentCards()
        {
            var topCards = _game.StudentsDeck.GetRange(0, _game.Players.Count);
            _game.StudentsDeck.RemoveRange(0, _game.Players.Count);
            _game.StudentsInPlay.AddRange(topCards);
        }

        private static void FlipNewBuildingCards()
        {
            var topCards = _game.BuildingsDeck.GetRange(0, 8);
            _game.BuildingsDeck.RemoveRange(0, 8);
            _game.BuildingsInPlay.AddRange(topCards);
        }

        private static void GiveRandomChancellorToEachPlayer()
        {
            var chancellors = CreateChancellors();
            chancellors.Shuffle<Chancellor>();

            for(var i = 0; i < _game.Players.Count; i++)
            {
                _game.Players.ElementAt(i).Chancellor = chancellors[i];
            }
        }

        private static void SetTurnProduction(Player player)
        {
            player.TurnProduction = new Dictionary<Category, int>();

            player.TurnProduction.Add(player.Chancellor.Production.ProductionType, player.Chancellor.Production.Amount);

            player.Buildings?.Select(b => b.Production).ToList().ForEach(bp =>
            {
                if(player.TurnProduction.ContainsKey(bp.ProductionType))
                {
                    player.TurnProduction[bp.ProductionType] = player.TurnProduction[bp.ProductionType] + bp.Amount;
                }
                else
                {
                    player.TurnProduction.Add(bp.ProductionType, bp.Amount);
                }
            });

            player.Students?.ForEach(s =>
            {
                if (player.TurnProduction.ContainsKey(s.Production.ProductionType))
                {
                    player.TurnProduction[s.Production.ProductionType] = player.TurnProduction[s.Production.ProductionType] + s.Production.Amount;
                }
                else
                {
                    player.TurnProduction.Add(s.Production.ProductionType, s.Production.Amount);
                }
            });

            foreach (KeyValuePair<Category, int> kvp in player.TurnProduction)
            {
                //textBox3.Text += ("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
                Log.Information($"TURN PRODUCTION: {player.Name} produces...");
                Log.Information($"Key = {kvp.Key}, Value = {kvp.Value}");
            }
        }

        private static void PickCard(Player player)
        {
            var choices = new Choices()
            {
                Students = new List<Student>(),
                Buildings = new List<Building>()
            };

            _game.StudentsInPlay.ForEach(student =>
            {
                if (player.TurnProduction.ContainsKey(student.Cost.CostType) && student.Cost.Amount <= player.TurnProduction[student.Cost.CostType])
                {
                    choices.Students.Add(student);
                }
            });

            _game.BuildingsInPlay.ForEach(building =>
            {
                if (player.TurnProduction.ContainsKey(building.Production.ProductionType) && building.Cost <= player.Coins)
                {
                    choices.Buildings.Add(building);
                }
            });

            Random rnd = new Random();
            int month = rnd.Next(1, 2);
        }

        private static List<Player> CreatePlayers()
        {
            var players = new List<Player>
            {
                new Player("Noah"),
                new Player("Adam"),
                new Player("Ashley"),
                new Player("Debbie"),
            };

            return players;
        }

        private static List<Student> CreateStudents()
        {
            var students = new List<Student>
            {
                new Student("Nick Alvarez", 1, Category.Fun, 1, Category.Academics),
                new Student("Bongo Smoker", 1, Category.Fun, 1, Category.Academics),
                new Student("Ima Belcher", 1, Category.Fun, 1, Category.Academics),
                new Student("Pardi Hardi", 1, Category.Fun, 1, Category.Academics),
                new Student("Joe King", 1, Category.Fun, 1, Category.Academics),
                new Student("Kay Oss", 1, Category.Fun, 1, Category.Academics),
                new Student("Holly Wood", 1, Category.Fun, 1, Category.Academics),
                new Student("Buck Nekkid", 1, Category.Fun, 1, Category.Academics),
            };

            return students;
        }

        private static List<Building> CreateBuildings()
        {
            var buildings = new List<Building>
            {
                new Building("Dorms", 2, 1, Category.Fun),
                new Building("Dorms", 2, 1, Category.Fun),
                new Building("Dorms", 2, 1, Category.Fun),

                new Building("Tennis Court", 2, 1, Category.Fun),
                new Building("Tennis Court", 2, 1, Category.Fun),
                new Building("Tennis Court", 2, 1, Category.Fun),

                new Building("Food Court", 3, 2, Category.Fun),
                new Building("Food Court", 3, 2, Category.Fun),

                new Building("Rec Center", 3, 2, Category.Fun),
                new Building("Rec Center", 3, 2, Category.Fun),

                new Building("Performing Arts Center", 4, 3, Category.Fun),
                new Building("Performing Arts Center", 4, 3, Category.Fun),

                new Building("Aquatic Center", 4, 3, Category.Fun),
                new Building("Aquatic Center", 4, 3, Category.Fun),
            };

            return buildings;
        }

        private static List<Chancellor> CreateChancellors()
        {
            var chancellors = new List<Chancellor>
            {
                new Chancellor("Coach Mcteach", 0, Category.None, 2, Category.Football),
                new Chancellor("Shear-Lock Combs", 1, Category.None, 2, Category.Beauty),
                new Chancellor("Pastor Tense", 1, Category.None, 2, Category.Academics),
                new Chancellor("Wayne Kerr", 1, Category.Fun, 2, Category.Fun),
                new Chancellor("Sum Ting Wong", 1, Category.Fun, 2, Category.Research),
                new Chancellor("Robyn Banks", 1, Category.Fun, 2, Category.Tuition)
            };

            return chancellors;
        }




    }

    public class Choices
    {
        public List<Student> Students { get; set; }
        public List<Building> Buildings { get; set; }
    }
}
