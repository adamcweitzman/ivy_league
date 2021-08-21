using System;
namespace ivy_league.Models
{
    public class Chancellor : Student
    {
        public Chancellor(string name, int cost, Category costType, int productionAmount, Category productionType) : base(name, cost, costType, productionAmount, productionType)
        {

        }
    }
}
