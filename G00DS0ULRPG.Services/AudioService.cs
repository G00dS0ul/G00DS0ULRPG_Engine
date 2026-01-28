using System.Windows;
using System.Collections.Generic;
using System.Media;
using System.Text;

namespace G00DS0ULRPG.Services
{
    public static class AudioService
    {
        public static void PlaySound(string soundFileName)
        {
            try
            {
                var path = $"{AppDomain.CurrentDomain.BaseDirectory}AudioFiles\\{soundFileName}";

                if (File.Exists(path))
                {
                    using var player = new SoundPlayer(path);
                    player.Play();
                }
            }
            catch (Exception ex)
            {
                
            }
        }
    }
}
