using G00DS0ULRPG.ViewModel;
using G00DS0ULRPG.Models;
using System.Windows;
using G00DS0ULRPG.Services;
using WPFUI.Services;

namespace WPFUI
{
    public partial class TraderScreen : Window
    {
        public GameSession Session => DataContext as GameSession;
        public TraderScreen()
        {
            InitializeComponent();
        }

        private void OnClick_Sell(object sender, RoutedEventArgs e)
        {
            var groupInventoryItem = ((FrameworkElement)sender).DataContext as GroupedInventoryItem;

            if (groupInventoryItem != null)
            {
                Session.CurrentPlayer.ReceiveGold(groupInventoryItem.Item.Price);
                Session.CurrentTrader.AddItemToInventory(groupInventoryItem.Item);
                Session.CurrentPlayer.RemoveItemFromInventory(groupInventoryItem.Item);

                AudioService.PlaySound("Trade_Sell.wav");
            }
        }

        private void OnClick_Buy(object sender, RoutedEventArgs e)
        {
            var groupedInventoryItem = ((FrameworkElement)sender).DataContext as GroupedInventoryItem;

            if(groupedInventoryItem != null)
            {
                if(Session.CurrentPlayer.Gold >= groupedInventoryItem.Item.Price)
                {
                    Session.CurrentPlayer.SpendGold(groupedInventoryItem.Item.Price);
                    Session.CurrentTrader.RemoveItemFromInventory(groupedInventoryItem.Item);
                    Session.CurrentPlayer.AddItemToInventory(groupedInventoryItem.Item);

                    AudioService.PlaySound("Trade.wav");
                }
                else
                {
                    MessageBox.Show("You do not Have Enough Gold For This Item!!!");
                }
            }
        }

        private void OnClick_Close(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
