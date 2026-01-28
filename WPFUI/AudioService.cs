using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Media;

namespace WPFUI
{
    public static class AudioService
    {
        private static MediaPlayer _mediaPlayer = new MediaPlayer();
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

        public static void PlayBackgroundMusic(string soundFileName)
        {
            try
            {
                var path = $"{AppDomain.CurrentDomain.BaseDirectory}AudioFiles\\{soundFileName}";


                if (!File.Exists(path))
                {
                    System.Windows.MessageBox.Show($"File not Found:\n{path}");
                }

                _mediaPlayer.Open(new Uri(path));

                _mediaPlayer.Volume = 1.0;

                // 👇 NEW: If the song starts, tell us!
                _mediaPlayer.MediaOpened += (s, e) =>
                {
                    // If you see this, the MP3 is healthy!
                    MessageBox.Show("SUCCESS: Music Loaded! You should hear it now.");
                };

                // 👇 NEW: If the song crashes, tell us WHY!
                _mediaPlayer.MediaFailed += (s, e) =>
                {
                    MessageBox.Show($"MEDIA FAILED: {e.ErrorException.Message}");
                };

                _mediaPlayer.MediaEnded += (sender, e) =>
                {
                    _mediaPlayer.Position = TimeSpan.Zero;
                    _mediaPlayer.Play();
                };

                _mediaPlayer.Play();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error playing music: {ex.Message}");
            }
        }
    }
}
