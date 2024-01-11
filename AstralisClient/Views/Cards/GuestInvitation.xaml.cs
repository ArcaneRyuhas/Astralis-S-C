using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Astralis.Views.Cards
{
    public partial class GuestInvitation : Window
    {
        private const int MAX_FIELDS_LENGHT = 10;

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

        private void TextLimeterForCode(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text.Length >= MAX_FIELDS_LENGHT)
            {
                e.Handled = true;
            }
        }
    }
}
