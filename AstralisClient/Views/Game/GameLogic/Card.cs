using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astralis.Views.Game.GameLogic
{

    public interface ICardPrototype
    {
        Card Clone();
    }

    public class Card: ICardPrototype
    {
        private const string TANK = "Tank";
        private const string MAGE = "Mage";

        public int Mana { get; set; }
        public int Attack { get; set; }
        public int Health { get; set; }
        public string Type { get; set; }

        public Card() { }

        public Card(int mana, int attack, int health, string type)
        {
            Mana = mana;
            Attack = attack;
            Health = health;
            Type = type;
        }

        public Card Clone()
        {
            return new Card(Mana, Attack, Health, Type);
        }

        public void TakeDamage(int damage)
        {
            if (this.Type.Equals(TANK))
            {
                Health -= (damage - 1);
            }
            else 
            {
                Health -= damage;
            }

            Health = Math.Max(0, Health);
        }

        public int DealDamage(int mageCount)
        {
            int dealtDamage;

            if (!this.Type.Equals(MAGE))
            {
                dealtDamage = Attack + mageCount;
            }
            else
            {
                dealtDamage = Attack;
            }

            return dealtDamage;
        }
    }
}
