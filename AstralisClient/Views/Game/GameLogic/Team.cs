using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astralis.Views.Game.GameLogic
{
    public class Team : INotifyPropertyChanged
    {
        private int roundMana;
        private int mana;
        private int health;

        public event PropertyChangedEventHandler PropertyChanged;

        public Team(int mana, int health)
        {
            roundMana = mana;
            Mana = mana;
            Health = health;
        }

        public int RoundMana { get { return roundMana; } set { roundMana = value; } }

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

        public int Health
        {
            get { return health; }
            set
            {
                if (health != value)
                {
                    health = value;
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
