using System;

namespace ivy_league.Models
{
    public class Building
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Cost { get; set; }
        public Production Production { get; set; }

        public Building(string name, int cost, int productionAmount, Category productionType)
        {
            Id = System.Guid.NewGuid().ToString();
            Name = name;
            Cost = cost;
            Production = new Production
            {
                Amount = productionAmount,
                ProductionType = productionType
            };
        }
    }



}
