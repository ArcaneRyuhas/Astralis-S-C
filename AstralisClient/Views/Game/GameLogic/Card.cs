using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Astralis.Views.Game.GameLogic
{

    public interface ICardPrototype
    {
        Card Clone();
    }

    public class Card: ICardPrototype, INotifyPropertyChanged
    {
        private int mana;
        private int attack;
        private int health;
        private string type;

        public event PropertyChangedEventHandler PropertyChanged;

        public Card() { }

        public Card(int mana, int attack, int health, string type)
        {
            Mana = mana;
            Attack = attack;
            Health = health;
            this.type = type;
        }

        public Card Clone()
        {
            return new Card(Mana, Attack, Health, Type);
        }

        public int Mana 
        { 
            get { return mana; } 
            set 
            { 
                if (mana != value) 
                {
                    mana = value;
                    OnPropertyChanged(nameof(Mana));
                } 
            } 
        }

        public int Attack 
        { 
            get { return attack; } 
            set 
            { 
                attack = value; 
                OnPropertyChanged(nameof(Attack));
            } 
        }

        public int Health 
        { 
            get { return health; } 

            set 
            { 
                health = value;
                OnPropertyChanged(nameof(Health));
            } 
        }

        public string Type { get { return type; }}

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void TakeDamage(int damage)
        {
            if (Type.Equals(Constants.TANK))
            {
                health -= (damage - 1);
            }
            else 
            {
                health -= damage;
            }

            Health = Math.Max(0, Health);
        }

        public int DealDamage(int mageCount)
        {
            int dealtDamage;

            if (Type.Equals(Constants.MAGE))
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
