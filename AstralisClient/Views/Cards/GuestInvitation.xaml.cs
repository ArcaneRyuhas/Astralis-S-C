using System;
using System.Windows;

namespace Astralis.Views.Cards
{
    public partial class GuestInvitation : Window
    {

        public event EventHandler<string> OnSubmit;
        public GuestInvitation()
        {
            InitializeComponent();
        }

        private void BtnJoinClick(object sender, RoutedEventArgs e)
        {
            string invitationCode = txtInvitationCode.Text;

            OnSubmit?.Invoke(this, invitationCode);
            this.Close();
        }
    }
}
