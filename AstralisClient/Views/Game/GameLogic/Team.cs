using System;
using System.ComponentModel;

namespace Astralis.Views.Game.GameLogic
{
    public class Team : INotifyPropertyChanged
    {
        private int _mana;
        private int _health;

        public int RoundMana { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public Team(int mana, int health)
        {
            RoundMana = mana;
            Mana = mana;
            Health = health;
        }

        public int Mana
        {
            get 
            { 
                return _mana; 
            }

            set
            {
                if (_mana != value)
                {
                    _mana = value;
                    OnPropertyChanged(nameof(Mana));
                }
            }
        }

        public int Health
        {
            get 
            { 
                return _health; 
            }

            set
            {
                if (_health != value)
                {
                    _health = value;
                    OnPropertyChanged(nameof(Health));
                }
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ReceiveDamage(int damage)
        {
            Health -= damage;
            Health = Math.Max(0, Health);
        }

        public bool UseMana(int manaCost)
        {
            bool canUseMana = false;
            Mana -= manaCost;

            if(Mana >= 0)
            {
                Mana = Math.Max(0, Mana);

                canUseMana = true;
            }
            else
            {
                Mana += manaCost;
            }
            
            return canUseMana;
        }
    }
}
