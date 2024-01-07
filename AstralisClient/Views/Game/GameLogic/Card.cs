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
        private int _mana;
        private int _attack;
        private int _health;
        private readonly string _type;

        public event PropertyChangedEventHandler PropertyChanged;

        public Card() { }

        //Due to the attributes of the card it is easy to understand the parameters and necessary to add all its attributes at the same time.
        public Card(int mana, int attack, int health, string type)
        {
            Mana = mana;
            Attack = attack;
            Health = health;
            _type = type;
        }

        public Card Clone()
        {
            return new Card(Mana, Attack, Health, Type);
        }

        public int Mana 
        { 
            get { return _mana; } 
            set 
            { 
                if (_mana != value) 
                {
                    _mana = value;
                    OnPropertyChanged(nameof(Mana));
                } 
            } 
        }

        public int Attack 
        { 
            get { return _attack; } 
            set 
            { 
                _attack = value; 
                OnPropertyChanged(nameof(Attack));
            } 
        }

        public int Health 
        { 
            get { return _health; } 

            set 
            { 
                _health = value;
                OnPropertyChanged(nameof(Health));
            } 
        }

        public string Type { get { return _type; }}

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void TakeDamage(int damage)
        {
            if (Type.Equals(Constants.TANK))
            {
                _health -= (damage - 1);
            }
            else 
            {
                _health -= damage;
            }

            Health = Math.Max(0, Health);
        }

        public int DealDamage(int mageCount)
        {
            int dealtDamage;

            if (Type.Equals(Constants.MAGE) && Attack != Constants.DEAD_DAMAGE)
            {
                dealtDamage = Attack + mageCount;
            }
            else
            {
                dealtDamage = Attack;
            }

            return dealtDamage;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Card otherCard = (Card)obj;

            return _mana == otherCard._mana
                && _attack == otherCard._attack
                && _health == otherCard._health
                && _type == otherCard._type;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (_type != null ? _type.GetHashCode() : 0);
                return hash;
            }
        }
    }
}
