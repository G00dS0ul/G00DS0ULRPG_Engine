using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Specialized;
using G00DS0ULRPG.ViewModel;
using G00DS0ULRPG.Models;
using G00DS0ULRPG.Services;
using Microsoft.Win32;
using WPFUI.Windows;

namespace WPFUI
{
    public partial class MainWindow : Window
    {
        private const string SAVE_GAME_FILE_EXTENSION = "g00ds0ulrpg";

        private GameSession? _gameSession;
        private readonly Dictionary<Key, Action>? _userInputActions = new Dictionary<Key, Action>();
        private Point? _dragStart;

        public MainWindow(Player player, int xLocation = 0, int yLocation = 0)
        {
            InitializeComponent();

            InitializeUserInputActions();

            SetActiveGameSessionTo(new GameSession(player, 0, 0));


            foreach (UIElement element in GameCanvas.Children)
            {
                if (element is Canvas)
                {
                    element.MouseDown += GameCanvas_OnMouseDown;
                    element.MouseMove += GameCanvas_OnMouseMove;
                    element.MouseUp += GameCanVas_OnMouseUp;
                }
            }
        }

       
        private void OnClick_MoveNorth(object sender, RoutedEventArgs e)
        {
            AudioService.PlaySound("button.wav");

            _gameSession?.MoveNorth();
        }
        private void OnClick_MoveWest(object sender, RoutedEventArgs e)
        {
            AudioService.PlaySound("button.wav");

            _gameSession?.MoveWest();
        }
        private void OnClick_MoveEast(object sender, RoutedEventArgs e)
        {
            AudioService.PlaySound("button.wav");

            _gameSession?.MoveEast();
        }
        private void OnClick_MoveSouth(object sender, RoutedEventArgs e)
        {
            AudioService.PlaySound("button.wav");

            _gameSession?.MoveSouth();
        }

        private void OnClick_AttackMonster(object sender, RoutedEventArgs e)
        {
            _gameSession?.AttackCurrentMonster();

            AudioService.PlaySound("Audio_Attack.wav");
        }

        private void OnClick_UseCurrentConsumable(object sender, RoutedEventArgs e)
        {
            AudioService.PlaySound("Audio_Consumables.wav");

            _gameSession?.UseCurrentConsumable();
        }

        private void OnClick_DisplayTradeScreen(object sender, RoutedEventArgs e)
        {
            if (_gameSession?.CurrentTrader != null)
            {
                var tradeScreen = new TraderScreen
                {
                    Owner = this,
                    DataContext = _gameSession
                };

                AudioService.PlaySound("Button2.wav");

                tradeScreen.ShowDialog();
            }
        }

        private void OnClick_Craft(object sender, RoutedEventArgs e)
        {
            var recipe = ((FrameworkElement)sender).DataContext as Recipe;
            _gameSession?.CraftItemUsing(recipe);

            AudioService.PlaySound("Audio_Craft.wav");
        }

        private void InitializeUserInputActions()
        {
            _userInputActions?.Add(Key.W, () => _gameSession?.MoveNorth());
            _userInputActions?.Add(Key.A, () => _gameSession?.MoveWest());
            _userInputActions?.Add(Key.S, () => _gameSession?.MoveSouth());
            _userInputActions?.Add(Key.D, () => _gameSession?.MoveEast());
            _userInputActions?.Add(Key.Z, () =>
            {
                
                _gameSession?.AttackCurrentMonster();

                AudioService.PlaySound("Audio_Attack.wav");
            });
            _userInputActions?.Add(Key.C, () => _gameSession?.UseCurrentConsumable());
            _userInputActions?.Add(Key.P, () => _gameSession?.PlayerDetails.IsVisible = !_gameSession.PlayerDetails.IsVisible);
            _userInputActions?.Add(Key.I, () => _gameSession?.InventoryDetails.IsVisible = !_gameSession.InventoryDetails.IsVisible);
            _userInputActions?.Add(Key.Q, () => _gameSession?.QuestDetails.IsVisible = !_gameSession.QuestDetails.IsVisible);
            _userInputActions?.Add(Key.R, () => _gameSession?.RecipesDetails.IsVisible = !_gameSession.RecipesDetails.IsVisible);
            _userInputActions?.Add(Key.M, () => _gameSession?.GameMessageDetails.IsVisible = !_gameSession.GameMessageDetails.IsVisible);
            _userInputActions?.Add(Key.T, () => OnClick_DisplayTradeScreen(this, new RoutedEventArgs()));
        }

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.S && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                SaveGame();
                e.Handled = true;
                return;
            }
            if (_userInputActions.ContainsKey(e.Key))
            {
                _userInputActions[e.Key].Invoke();

                e.Handled = true;
            }
        }

        private void SetActiveGameSessionTo(GameSession gameSession)
        {
            if (_gameSession != null)
            {
                _gameSession.GameMessages.CollectionChanged -= GameMessages_CollectionChanged;
            }

            _gameSession = gameSession;
            AudioService.PlayBackgroundMusic("BackgroundMusic.mp3");

            _gameSession.PlayerKilled += (sender, args) =>
            {
                AudioService.PlaySound("Button_Exit.wav");
                MessageBox.Show($"Game Over!!!");

            };
            _gameSession.MonsterKilled += (sender, args) =>
            {
                AudioService.PlaySound("Trade.wav");
                MessageBox.Show($"Victory!!!");
            };
            DataContext = _gameSession;

            _gameSession.GameMessages.CollectionChanged += GameMessages_CollectionChanged;
        }

        private void GameMessages_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            (GameMessagesFlowDocumentScrollViewer
                .Template
                .FindName("PART_ContentHost", GameMessagesFlowDocumentScrollViewer) as ScrollViewer)
                ?.ScrollToEnd();
        }

        private void StartNewGame_OnClick(object sender, RoutedEventArgs e)
        {
            _gameSession?.Dispose();

            AudioService.PlaySound("button.wav");

            var startup = new Startup();
            startup.Show();
            Close();
        }

        private void SaveGame_OnClick(object sender, RoutedEventArgs e)
        {
            AudioService.PlaySound("Button2.wav");

            SaveGame();
        }

        private void Exit_OnClick(object sender, RoutedEventArgs e)
        {
            AudioService.PlaySound("button.wav");

            Close();
        }

        private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
        {
            AskToSaveGame(e);
        }

        private void AskToSaveGame(CancelEventArgs e)
        {
            var message = new YesNoWindow("Save Game", "Do you want to save your game?")
            {
                Owner = GetWindow(this)
            };
            message.ShowDialog();

            if (message.ClickedYes)
            {
                AudioService.PlaySound("Button_Exit.wav");

                SaveGame();
            }

            else if (message.ClickedNo)
            {
                AudioService.PlaySound("Button_Exit.wav");

                Thread.Sleep(1000);
            }

            else
            {
                e.Cancel = true;
            }


        }


        private void SaveGame()
        {
            var saveFileDialog = new SaveFileDialog
            {
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory,
                Filter = $"Save game (*.{SAVE_GAME_FILE_EXTENSION})|*.{SAVE_GAME_FILE_EXTENSION}"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                SaveGameService.Save(new GameState(_gameSession?.CurrentPlayer, _gameSession.CurrentLocation.XCoordinate, _gameSession.CurrentLocation.YCoordinate), saveFileDialog.FileName);
            }
        }

        private void ClosePlayerDetailsWindow_OnClick(object sender, RoutedEventArgs e)
        {
            _gameSession?.PlayerDetails.IsVisible = false;
        }

        private void CloseInventoryWindow_OnClick(object sender, RoutedEventArgs e)
        {
            _gameSession?.InventoryDetails.IsVisible = false;
        }

        private void CloseQuestsWindow_OnClick(object sender, RoutedEventArgs e)
        {
            _gameSession?.QuestDetails.IsVisible = false;
        }

        private void CloseRecipesWindow_OnClick(object sender, RoutedEventArgs e)
        {
            _gameSession?.RecipesDetails.IsVisible = false;
        }

        private void CloseGameMessagesDetailsWindow_OnClick(object sender, RoutedEventArgs e)
        {
            _gameSession?.GameMessageDetails.IsVisible = false;
        }

        private void GameCanvas_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            var movingElement = (UIElement)sender;
            _dragStart = e.GetPosition(movingElement);
            movingElement.CaptureMouse();

            e.Handled = true;
        }

        private void GameCanvas_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_dragStart == null || e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }
            var mousePosition = e.GetPosition(GameCanvas);
            var movingElement = (UIElement)sender;

            if (mousePosition.X < _dragStart.Value.X || mousePosition.Y < _dragStart.Value.Y || mousePosition.X > GameCanvas.ActualWidth - ((Canvas)movingElement).ActualWidth + _dragStart.Value.X || mousePosition.Y > GameCanvas.ActualHeight - ((Canvas)movingElement).ActualHeight + _dragStart.Value.Y)
            {
                return;
            }

            Canvas.SetLeft(movingElement, mousePosition.X - _dragStart.Value.X);
            Canvas.SetTop(movingElement, mousePosition.Y - _dragStart.Value.Y);

            e.Handled = true;
        }

        private void GameCanVas_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            var movingElement = (UIElement)sender;
            movingElement.ReleaseMouseCapture();
            _dragStart = null;

            e.Handled = true;
        }
    }
}