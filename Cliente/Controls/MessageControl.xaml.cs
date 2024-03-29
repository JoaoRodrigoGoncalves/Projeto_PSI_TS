﻿using System;
using System.Windows;
using System.Windows.Controls;

namespace Cliente
{
    /// <summary>
    /// Interaction logic for MessageControl.xaml
    /// </summary>
    public partial class MessageControl : UserControl
    {
        private bool profileLocked = false;
        private uint userID;

        public MessageControl(uint userID, string name, uint? userImage, DateTime messageDateTime, string message, int? maxWidth = null, bool blockProfile = false)
        {
            InitializeComponent();
            this.textBlock_nomeUtilizador.Text = name;
            this.image_ImagemUtilizador.ImageSource = Utilities.getImage(userImage);
            this.textBlock_timestamp.Text = messageDateTime.ToString("G");
            this.textBlock_mensagem.Text = message;
            this.userID = userID;

            /// Esta condição é necessária por causa da janela <see cref="UserProfile"/>, de forma a que o controlo caiba no stackpanel
            if (maxWidth.HasValue)
                this.BallonBorder.MaxWidth = Convert.ToDouble(maxWidth);

            profileLocked = blockProfile;
        }

        private void textBlock_nomeUtilizador_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!this.profileLocked)
            {
                UserProfile profile = new UserProfile(userID);
                profile.ShowDialog();
            }
        }

        private void textBlock_nomeUtilizador_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            textBlock_nomeUtilizador.TextDecorations = TextDecorations.Underline;
        }

        private void textBlock_nomeUtilizador_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            TextDecorationCollection decorations = new TextDecorationCollection();
            decorations.Clear();
            textBlock_nomeUtilizador.TextDecorations = decorations;
        }
    }
}
