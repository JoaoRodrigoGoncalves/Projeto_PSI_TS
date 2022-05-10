using System;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Media.Imaging;

namespace Cliente
{
    internal class Utilities
    {
        internal static BitmapImage getImage(uint? userImage)
        {
            uint?[] imgs = { 1, 2, 3, 4, 5 };
            if (userImage == null || !imgs.Contains(userImage))
            {
                return new BitmapImage(new Uri("pack://application:,,,/Resources/userIcon.png"));
            }
            else
            {
                return new BitmapImage(new Uri("pack://application:,,,/Resources/userIcon" + userImage + ".png"));
            }
        }
    }
}
