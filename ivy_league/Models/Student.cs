using System;

namespace ivy_league.Models
{
    public class Student
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Cost Cost { get; set; }
        public Production Production { get; set; }

        public Student(string name, int cost, Category costType, int productionAmount, Category productionType)
        {
            Id = System.Guid.NewGuid().ToString();
            Name = name;
            Cost = new Cost
            {
                Amount = cost,
                CostType = costType
            };
            Production = new Production
            {
                Amount = productionAmount,
                ProductionType = productionType
            };
        }
    }

    public class Cost
    {
        public int Amount { get; set; }
        public Category CostType { get; set; }
    }
}
