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

            while (_game.StudentsDeck.Count != 0 && _game.BuildingsDeck.Count != 0)
            {
                foreach (var player in _game.Players)
                {
                    TakeTurn(player);
                };
            }
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
                Log.Information($"TURN PRODUCTION: {player.Name} produces {kvp.Value} {kvp.Key}");
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
                if (building.Cost <= player.Coins)
                {
                    choices.Buildings.Add(building);
                }
            });


            var numberOfChoices = choices.Buildings.Count + choices.Students.Count;

            Log.Information($"CHOICES: {player.Name} has {numberOfChoices} choices");

            if (choices.Buildings.Any() && choices.Students.Any())
            {
                Random rnd = new Random();
                int random = rnd.Next(1, 3);
                if(random == 1)
                {
                    AddBuildingToPlayer(player, choices);
                }
                else
                {
                    AddStudentToPlayer(player, choices);
                }
            }
            else if(choices.Buildings.Any())
            {
                AddBuildingToPlayer(player, choices);
            }
            else if(choices.Students.Any())
            {
                AddStudentToPlayer(player, choices);
            }
        }

        private static void AddStudentToPlayer(Player player, Choices choices)
        {
            var studentChoice = choices.Students.GetRandomFromList();

            player.Students.Add(studentChoice);
            _game.StudentsInPlay.RemoveAll(building => building.Id == studentChoice.Id);

            var tuition = player.Tuition;

            if(player.TurnProduction.ContainsKey(Category.Tuition))
            {
                tuition += player.TurnProduction[Category.Tuition];
            }

            player.Coins += tuition;

            Log.Information($"STUDENT CHOICE: {player.Name} chooses {studentChoice.Name} and gets {tuition} coins for tuition");

            ReplenishStudents();
        }

        private static void AddBuildingToPlayer(Player player, Choices choices)
        {
            var buildingChoice = choices.Buildings.GetRandomFromList();
            player.Buildings.Add(buildingChoice);
            _game.BuildingsInPlay.RemoveAll(building => building.Id == buildingChoice.Id);

            Log.Information($"BUILDING CHOICE: {player.Name} chooses {buildingChoice.Name}");

            ReplenishBuildings();
        }

        private static void ReplenishBuildings()
        {
            if(_game.BuildingsDeck.Any())
            {
                _game.BuildingsInPlay.Add(_game.BuildingsDeck[0]);
                _game.BuildingsDeck.RemoveAt(0);
            }
        }

        private static void ReplenishStudents()
        {
            if(_game.StudentsDeck.Any())
            {
                _game.StudentsInPlay.Add(_game.StudentsDeck[0]);
                _game.StudentsDeck.RemoveAt(0);
            }
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

                new Student("Abdi Abdullah", 1, Category.Football, 1, Category.Football),
                new Student("Shane Bell", 1, Category.Football, 1, Category.Football),
                new Student("Hugh Mungus", 1, Category.Football, 1, Category.Football),
                new Student("Dick Johnson", 1, Category.Football, 1, Category.Football),
                new Student("Corter Backman", 1, Category.Football, 1, Category.Football),
                new Student("Blue Mchipper", 1, Category.Football, 1, Category.Football),
                new Student("Junior Young Jr", 1, Category.Football, 1, Category.Football),
                new Student("Buddy Frienderson", 1, Category.Football, 1, Category.Football),

                new Student("Monique Newyork", 1, Category.Research, 1, Category.Academics),
                new Student("Bookish Mcgee", 1, Category.Research, 1, Category.Academics),
                new Student("Iota Studi", 1, Category.Research, 1, Category.Academics),
                new Student("Reed Chaucer", 1, Category.Research, 1, Category.Academics),
                new Student("Amanda Lynn", 1, Category.Research, 1, Category.Academics),
                new Student("Sarah Nader", 1, Category.Research, 1, Category.Academics),
                new Student("Tish Huges", 1, Category.Research, 1, Category.Academics),
                new Student("Research8", 1, Category.Research, 1, Category.Academics),

                new Student("Jack Pott", 1, Category.Tuition, 1, Category.Academics),
                new Student("Marsha Mellow", 1, Category.Tuition, 1, Category.Academics),
                new Student("Ty Coon", 1, Category.Tuition, 1, Category.Academics),
                new Student("Kay Bull", 1, Category.Tuition, 1, Category.Academics),
                new Student("Queen King", 1, Category.Tuition, 1, Category.Academics),
                new Student("Tuition6", 1, Category.Tuition, 1, Category.Academics),
                new Student("Tuition7", 1, Category.Tuition, 1, Category.Academics),
                new Student("Tuition8", 1, Category.Tuition, 1, Category.Academics),
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

                new Building("Law School", 2, 1, Category.Research),
                new Building("Law School", 2, 1, Category.Research),
                new Building("Law School", 2, 1, Category.Research),
                new Building("Biosciences Center", 2, 1, Category.Research),
                new Building("Biosciences Center", 2, 1, Category.Research),
                new Building("Biosciences Center", 2, 1, Category.Research),
                new Building("Engineering Quadrangle", 3, 2, Category.Research),
                new Building("Engineering Quadrangle", 3, 2, Category.Research),
                new Building("Nanosystems Laboratory", 3, 2, Category.Research),
                new Building("Nanosystems Laboratory", 3, 2, Category.Research),
                new Building("Planetarium", 4, 3, Category.Research),
                new Building("Planetarium", 4, 3, Category.Research),
                new Building("Particle Accelerator", 4, 3, Category.Research),
                new Building("Particle Accelerator", 4, 3, Category.Research),

                new Building("Bookstore", 2, 1, Category.Tuition),
                new Building("Bookstore", 2, 1, Category.Tuition),
                new Building("Bookstore", 2, 1, Category.Tuition),
                new Building("Student Union", 2, 1, Category.Tuition),
                new Building("Student Union", 2, 1, Category.Tuition),
                new Building("Student Union", 2, 1, Category.Tuition),
                new Building("Internship Program", 3, 2, Category.Tuition),
                new Building("Internship Program", 3, 2, Category.Tuition),
                new Building("Bursers Office", 3, 2, Category.Tuition),
                new Building("Bursers OFfice", 3, 2, Category.Tuition),
                new Building("Deans Office", 4, 3, Category.Tuition),
                new Building("Deans Office", 4, 3, Category.Tuition),
                new Building("Business School", 4, 3, Category.Tuition),
                new Building("Business School", 4, 3, Category.Tuition),

                new Building("Fountains", 2, 1, Category.Beauty),
                new Building("Fountains", 2, 1, Category.Beauty),
                new Building("Fountains", 2, 1, Category.Beauty),
                new Building("Secret Hummingbird Garden", 2, 1, Category.Beauty),
                new Building("Secret Hummingbird Garden", 2, 1, Category.Beauty),
                new Building("Secret Hummingbird Garden", 2, 1, Category.Beauty),
                new Building("Turtle Pond", 3, 2, Category.Beauty),
                new Building("Turtle Pond", 3, 2, Category.Beauty),
                new Building("Sculpture Garden", 3, 2, Category.Beauty),
                new Building("Sculpture Garden", 3, 2, Category.Beauty),
                new Building("Japanese Tea Garden", 4, 3, Category.Beauty),
                new Building("Japanese Tea Garden", 4, 3, Category.Beauty),
                new Building("Outdoor Ampitheatre", 4, 3, Category.Beauty),
                new Building("Outdoor Ampitheatre", 4, 3, Category.Beauty),
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
