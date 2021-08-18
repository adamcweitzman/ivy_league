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
        private Serilog.Core.Logger _log;

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

            while (_game.StudentsDeck.Count != 0)
            {
                foreach (var player in _game.Players)
                {
                    TakeTurn(player);
                };
            }
        }

        private static void TakeTurn(Player player)
        {
            var choices = GetNumberOfChoices(player);
        }

        private static void Setup()
        {
            _game.BuildingsDeck.Shuffle<Building>();
            _game.StudentsDeck.Shuffle<Student>();

            FlipNewStudentCards();
            FlipNewBuildingCards();
            GiveRandomBuildingToEachPlayer();

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

        private static void GiveRandomBuildingToEachPlayer()
        {

        }

        private static int GetNumberOfChoices(Player player)
        {
            // TODO: handle the beggining of game where player has no buildings

            var buildingProduction = player.Buildings.Select(p => p.Production);
            var studentChoices = _game.StudentsInPlay.Count(
                s => buildingProduction.Any(
                    bp => bp.ProductionType == s.Cost.CostType &&
                        bp.Amount == s.Cost.Amount));

            var buildingChoices = _game.BuildingsInPlay.Where(b => b.Cost <= player.Coins).Count();

            return buildingChoices;
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
    }
}
