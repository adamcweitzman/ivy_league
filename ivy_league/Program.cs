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
        //TODO problem with players getting stuck with 0 choices

        private static Game _game;

        static void Main(string[] args)
        {
            var guid = Guid.NewGuid();
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File($"./{guid}-ivyleaguelog.log")
                .CreateLogger();

            var students = CreateStudents();
            var buildings = CreateBuildings();
            var players = CreatePlayers();

            _game = new Game(students, buildings, players);
            Play();
        }

        public static void Play()
        {
            Log.Information("Play Game");
            Setup();

            while (_game.Round < 25 && _game.StudentsDeck.Count != 0 && _game.BuildingsDeck.Count != 0)
            {
                Log.Information($"ROUND {_game.Round}");
                foreach(var student in _game.StudentsInPlay)
                {
                    Log.Information($"AVAILABLE STUDENT: {student.Name} : {student.Cost.CostType} : {student.Cost.Amount}");
                }
                foreach (var player in _game.Players)
                {
                    TakeTurn(player);
                };
                _game.Round++;
            }

            Log.Information("GAME END");

            foreach (var player in _game.Players)
            {
                var academics = player.Students.Where(s => s.Production.ProductionType == Category.Academics).Sum(s => s.Production.Amount);

                Log.Information($"   {player.Name} finishes with {player.Coins} coins, {academics} academics, {player.Football} football, {player.Students.Count} students, and {player.Buildings.Count} buildings");
            }

            int percentOfTurnsNoChoice = (int)((decimal)_game.TurnsWithNoChoice / ((decimal)_game.Round * 4) * 100);

            Log.Information($"   No Choice Percentage: {percentOfTurnsNoChoice}%");
        }

        private static void TakeTurn(Player player)
        {
            Log.Information($"{player.Name}'s turn...");

            SetTurnProduction(player);
            PickCard(player);
        }

        private static bool CheckIfFootballIsOption(Player player)
        {
            var index = _game.Players.IndexOf(player);
            Player opponentOne;
            Player opponentTwo;

            if (index == 0)
            {
                opponentOne = _game.Players[1];
                opponentTwo = _game.Players[3];

            }
            else if (index == 3)
            {
                opponentOne = _game.Players[0];
                opponentTwo = _game.Players[2];
            }
            else
            {
                opponentOne = _game.Players[index + 1];
                opponentTwo = _game.Players[index - 1];
            }

            if ((player.Football > opponentOne.Football && opponentOne.Coins > 0) || (player.Football > opponentTwo.Football && opponentTwo.Coins > 0))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void ExecuteFootball(Player player)
        {
            var index = _game.Players.IndexOf(player);
            Player opponentOne;
            Player opponentTwo;

            if(index == 0)
            {
                opponentOne = _game.Players[1];
                opponentTwo = _game.Players[3];

            }
            else if(index == 3)
            {
                opponentOne = _game.Players[0];
                opponentTwo = _game.Players[2];
            }
            else
            {
                opponentOne = _game.Players[index + 1];
                opponentTwo = _game.Players[index - 1];
            }

            if(player.Football > opponentOne.Football)
            {
                Log.Information($"  FOOTBALL: {player.Name} beats {opponentOne.Name} at football by a score of {player.Football} - {opponentOne.Football} and takes a coin");
                if(opponentOne.Coins > 0)
                {
                    opponentOne.Coins -= 1;
                    player.Coins += 1;
                }
                else
                {
                    Log.Information($"  FOOTBALL: {opponentOne.Name} has no coins though");
                }
            }

            if(player.Football > opponentTwo.Football)
            {
                Log.Information($"  FOOTBALL: {player.Name} beats {opponentTwo.Name} at football by a score of {player.Football} - {opponentTwo.Football} and takes a coin");
                if (opponentTwo.Coins > 0)
                {
                    Log.Information($"  FOOTBALL: {opponentTwo.Name} has no coins though");
                    opponentTwo.Coins -= 1;
                    player.Coins += 1;
                }
                else
                {
                    Log.Information($"  FOOTBALL: {opponentOne.Name} has no coins though");
                }
            }

            if(player.Football <= opponentTwo.Football && player.Football <= opponentOne.Football)
            {
                Log.Information($"  FOOTBALL: {player.Name}'s ({player.Football}) football team sucks and can't beat {opponentOne.Name} ({opponentOne.Football}) or {opponentTwo.Name} ({opponentTwo.Football})");
            }
        }

        private static void Setup()
        {
            _game.BuildingsDeck.Shuffle<Building>();
            _game.StudentsDeck.Shuffle<Student>();

            FlipNewStudentCards();
            FlipNewBuildingCards();
            GiveRandomChancellorToEachPlayer();

            _game.Players.ForEach(x => x.Coins = 6);
        }

        private static void FlipNewStudentCards()
        {
            var topCards = _game.StudentsDeck.GetRange(0, _game.Players.Count * 3);
            _game.StudentsDeck.RemoveRange(0, _game.Players.Count * 2);
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
                if(chancellors[i].Production.ProductionType == Category.Football)
                {
                    _game.Players.ElementAt(i).Football = 2;
                }
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
                Log.Information($"  TURN PRODUCTION: {player.Name} produces {kvp.Value} {kvp.Key}");
            }

            Log.Information($"  NUMBER OF STUDENTS: {player.Students.Count}");
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

            var isFootballAnOption = CheckIfFootballIsOption(player);

            var numberOfChoices = isFootballAnOption ? 1 : 0;

            numberOfChoices = numberOfChoices + choices.Buildings.Count + choices.Students.Count;

            if(numberOfChoices < 2)
            {
                _game.TurnsWithNoChoice += 1;
            }

            Log.Information($"  CHOICES: {player.Name} has {numberOfChoices} choices");
            Log.Information($"  COINS: {player.Name} has {player.Coins} coins");

            if (choices.Buildings.Any() && choices.Students.Any() && isFootballAnOption)
            {
                Random rnd = new Random();
                int random = rnd.Next(1, 4);
                switch(random)
                {
                    case 1:
                        AddStudentToPlayer(player, choices);
                        break;
                    case 2:
                        ExecuteFootball(player);
                        break;
                    case 3:
                        AddBuildingToPlayer(player, choices);
                        break;
                }
            }
            else if(choices.Buildings.Any() && choices.Students.Any())
            {
                AddStudentToPlayer(player, choices);
            }
            else if(choices.Students.Any())
            {
                AddStudentToPlayer(player, choices);
            }
            else if(choices.Buildings.Any())
            {
                AddBuildingToPlayer(player, choices);
            }
            else if(isFootballAnOption)
            {
                ExecuteFootball(player);
            }

        }

        private static void GivePlayerFinancialAid(Player player)
        {
            player.Coins += 1;
        }

        private static void AddStudentToPlayer(Player player, Choices choices)
        {
            Student studentChoice;

            if(choices.Students.Any(student => student.Production.ProductionType == Category.Football))
            {
                studentChoice = choices.Students.First(student => student.Production.ProductionType == Category.Football);
            }
            else
            {
                studentChoice = choices.Students.GetRandomFromList();
            }

            player.Students.Add(studentChoice);
            _game.StudentsInPlay.RemoveAll(building => building.Id == studentChoice.Id);

            var tuition = player.Tuition;

            if(player.TurnProduction.ContainsKey(Category.Tuition))
            {
                tuition += player.TurnProduction[Category.Tuition];
            }

            player.Coins += tuition;

            Log.Information($"  STUDENT CHOICE: {player.Name} chooses {studentChoice.Name} and gets {tuition} coins for tuition");

            if (studentChoice.Production.ProductionType == Category.Football)
            {
                player.Football = studentChoice.Production.Amount;
            }

            ReplenishStudents();
        } 

        private static void AddBuildingToPlayer(Player player, Choices choices)
        {
            var buildingChoice = choices.Buildings.GetRandomFromList();
            player.Buildings.Add(buildingChoice);
            _game.BuildingsInPlay.RemoveAll(building => building.Id == buildingChoice.Id);
            player.Coins = player.Coins - buildingChoice.Cost;

            Log.Information($"  BUILDING CHOICE: {player.Name} chooses {buildingChoice.Name}");

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
                new Student("Nick Alvarez", 3, Category.Fun, 2, Category.Academics),
                new Student("Bongo Smoker", 3, Category.Fun, 2, Category.Academics),
                new Student("Ima Belcher", 3, Category.Fun, 2, Category.Academics),
                new Student("Pardi Hardi", 3, Category.Fun, 2, Category.Academics),
                new Student("Joe King", 4, Category.Fun, 3, Category.Academics),
                new Student("Kay Oss", 4, Category.Fun, 3, Category.Academics),
                new Student("Holly Wood", 4, Category.Fun, 3, Category.Academics),
                new Student("Buck Nekkid", 6, Category.Fun, 4, Category.Academics),

                new Student("Abdi Abdullah", 3, Category.Fun, 2, Category.Football),
                new Student("Shane Bell", 3, Category.Fun, 2, Category.Football),
                new Student("Hugh Mungus", 3, Category.Beauty, 2, Category.Football),
                new Student("Dick Johnson", 3, Category.Beauty, 2, Category.Football),
                new Student("Corter Backman", 4, Category.Research, 3, Category.Football),
                new Student("Blue Mchipper", 4, Category.Research, 3, Category.Football),
                new Student("Junior Young Jr", 4, Category.Football, 3, Category.Football),
                new Student("Buddy Frienderson", 6, Category.Football, 4, Category.Football),

                new Student("Monique Newyork", 3, Category.Research, 2, Category.Academics),
                new Student("Bookish Mcgee", 3, Category.Research, 2, Category.Academics),
                new Student("Iota Studi", 3, Category.Research, 2, Category.Academics),
                new Student("Reed Chaucer", 3, Category.Research, 2, Category.Academics),
                new Student("Amanda Lynn", 4, Category.Research, 3, Category.Academics),
                new Student("Sarah Nader", 4, Category.Research, 3, Category.Academics),
                new Student("Tish Huges", 4, Category.Research, 3, Category.Academics),
                new Student("My Croscope", 6, Category.Research, 4, Category.Academics),

                new Student("Jack Pott", 3, Category.Beauty, 2, Category.Academics),
                new Student("Marsha Mellow", 3, Category.Beauty, 2, Category.Academics),
                new Student("Ty Coon", 3, Category.Beauty, 2, Category.Academics),
                new Student("Kay Bull", 3, Category.Beauty, 2, Category.Academics),
                new Student("Queen King", 4, Category.Beauty, 3, Category.Academics),
                new Student("Hoosier Dadi", 4, Category.Beauty, 3, Category.Academics),
                new Student("Prince O' Egypt", 4, Category.Beauty, 3, Category.Academics),
                new Student("Richy Rich", 6, Category.Beauty, 4, Category.Academics),
            };

            return students;
        }

        private static List<Building> CreateBuildings()
        {
            var buildings = new List<Building>
            {
                new Building("Dorms", 1, 1, Category.Fun),
                new Building("Dorms", 1, 1, Category.Fun),
                new Building("Dorms", 1, 1, Category.Fun),
                new Building("Tennis Court", 1, 1, Category.Fun),
                new Building("Tennis Court", 1, 1, Category.Fun),
                new Building("Tennis Court", 1, 1, Category.Fun),
                new Building("Food Court", 2, 2, Category.Fun),
                new Building("Food Court", 2, 2, Category.Fun),
                new Building("Rec Center", 2, 2, Category.Fun),
                new Building("Rec Center", 2, 2, Category.Fun),
                new Building("Performing Arts Center", 3, 3, Category.Fun),
                new Building("Performing Arts Center", 3, 3, Category.Fun),
                new Building("Aquatic Center", 3, 3, Category.Fun),
                new Building("Aquatic Center", 3, 3, Category.Fun),

                new Building("Law School", 1, 1, Category.Research),
                new Building("Law School", 1, 1, Category.Research),
                new Building("Law School", 1, 1, Category.Research),
                new Building("Biosciences Center", 1, 1, Category.Research),
                new Building("Biosciences Center", 1, 1, Category.Research),
                new Building("Biosciences Center", 1, 1, Category.Research),
                new Building("Engineering Quadrangle", 2, 2, Category.Research),
                new Building("Engineering Quadrangle", 2, 2, Category.Research),
                new Building("Nanosystems Laboratory", 2, 2, Category.Research),
                new Building("Nanosystems Laboratory", 2, 2, Category.Research),
                new Building("Planetarium", 3, 3, Category.Research),
                new Building("Planetarium", 3, 3, Category.Research),
                new Building("Particle Accelerator", 3, 3, Category.Research),
                new Building("Particle Accelerator", 3, 3, Category.Research),

                new Building("Bookstore", 1, 1, Category.Tuition),
                new Building("Bookstore", 1, 1, Category.Tuition),
                new Building("Bookstore", 1, 1, Category.Tuition),
                new Building("Student Union", 1, 1, Category.Tuition),
                new Building("Student Union", 1, 1, Category.Tuition),
                new Building("Student Union", 1, 1, Category.Tuition),
                new Building("Internship Program", 2, 2, Category.Tuition),
                new Building("Internship Program", 2, 2, Category.Tuition),
                new Building("Bursers Office", 2, 2, Category.Tuition),
                new Building("Bursers OFfice", 2, 2, Category.Tuition),
                new Building("Deans Office", 3, 3, Category.Tuition),
                new Building("Deans Office", 3, 3, Category.Tuition),
                new Building("Business School", 3, 3, Category.Tuition),
                new Building("Business School", 3, 3, Category.Tuition),

                new Building("Fountains", 1, 1, Category.Beauty),
                new Building("Fountains", 1, 1, Category.Beauty),
                new Building("Fountains", 1, 1, Category.Beauty),
                new Building("Secret Hummingbird Garden", 1, 1, Category.Beauty),
                new Building("Secret Hummingbird Garden", 1, 1, Category.Beauty),
                new Building("Secret Hummingbird Garden", 1, 1, Category.Beauty),
                new Building("Turtle Pond", 2, 2, Category.Beauty),
                new Building("Turtle Pond", 2, 2, Category.Beauty),
                new Building("Sculpture Garden", 2, 2, Category.Beauty),
                new Building("Sculpture Garden", 2, 2, Category.Beauty),
                new Building("Japanese Tea Garden", 3, 3, Category.Beauty),
                new Building("Japanese Tea Garden", 3, 3, Category.Beauty),
                new Building("Outdoor Ampitheatre", 3, 3, Category.Beauty),
                new Building("Outdoor Ampitheatre", 3, 3, Category.Beauty),
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
