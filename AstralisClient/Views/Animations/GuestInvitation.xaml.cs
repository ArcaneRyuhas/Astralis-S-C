using System;
using System.Windows;

namespace Astralis.Views.Animations
{
    public partial class GuestInvitation : Window
    {

        public event EventHandler<string> OnSubmit;
        public GuestInvitation()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string invitationCode = tbInvitationCode.Text;

            OnSubmit?.Invoke(this, invitationCode);
            this.Close();
        }
    }
}
