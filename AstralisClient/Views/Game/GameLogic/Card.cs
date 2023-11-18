using System;

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

        private int mana;
        private int attack;
        private int health;
        private string type;

        public int Mana { get { return mana; } set { mana = value;} }
        public int Attack { get { return attack;} set { attack = value;} }
        public int Health { get { return health;} set { health = value;} }
        public string Type { get { return type; } set { type = value; } }

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
