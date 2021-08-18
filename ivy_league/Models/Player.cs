using System;
using System.Collections.Generic;

namespace ivy_league.Models
{
    public class Player
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<Student> Students { get; set; }
        public List<Building> Buildings { get; set; }
        public int Coins { get; set; }

        public Player(string name)
        {
            Name = name;
            Id = System.Guid.NewGuid().ToString();
        }
    }
}
