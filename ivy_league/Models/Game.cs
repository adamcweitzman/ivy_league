using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace ivy_league.Models
{
    public class Game
    {
        public List<Building> BuildingsDeck { get; set; }
        public List<Student> StudentsDeck { get; set; }
        public List<Building> BuildingsInPlay { get; set; }
        public List<Student> StudentsInPlay { get; set; }
        public List<Player> Players { get; set; }
        public int Round { get; set; }

        public Game(List<Student> students, List<Building> buildings, List<Player> players)
        {
            StudentsDeck = students;
            BuildingsDeck = buildings;
            Players = players;
            StudentsInPlay = new List<Student>();
            BuildingsInPlay = new List<Building>();
        }

        public void Play()
        {
            Setup();

            while(StudentsDeck.Count != 0)
            {
                foreach(var player in Players)
                {
                    TakeTurn(player);
                };
            }
        }

        private void TakeTurn(Player player)
        {
            var choices = GetNumberOfChoices(player);
        }

        private void Setup()
        {
            BuildingsDeck.Shuffle<Building>();
            StudentsDeck.Shuffle<Student>();

            FlipNewStudentCards();
            FlipNewBuildingCards();
            GiveRandomBuildingToEachPlayer();

            Players.ForEach(x => x.Coins = 3);
        }

        private void FlipNewStudentCards()
        {
            var topCards = StudentsDeck.GetRange(0, Players.Count);
            StudentsDeck.RemoveRange(0, Players.Count);
            StudentsInPlay.AddRange(topCards);
        }

        private void FlipNewBuildingCards()
        {
            var topCards = BuildingsDeck.GetRange(0, 8);
            BuildingsDeck.RemoveRange(0, 8);
            BuildingsInPlay.AddRange(topCards);
        }

        private void GiveRandomBuildingToEachPlayer()
        {

        }

        private int GetNumberOfChoices(Player player)
        {
            // TODO: handle the beggining of game where player has no buildings

            var buildingProduction = player.Buildings.Select(p => p.Production);
            var studentChoices = StudentsInPlay.Count(
                s => buildingProduction.Any(
                    bp => bp.ProductionType == s.Cost.CostType &&
                        bp.Amount == s.Cost.Amount));

            var buildingChoices = BuildingsInPlay.Where(b => b.Cost <= player.Coins).Count();

            return buildingChoices;
        }

    }
}

