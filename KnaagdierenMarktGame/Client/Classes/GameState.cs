using KnaagdierenMarktGame.Client.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KnaagdierenMarktGame.Client.Classes
{
    public class GameState
    {
        public delegate Task PropertyChanged(object message);

        public event PropertyChanged OnPropertyChanged;
        public List<AnimalCard> RemainingAuctionCards { get; set; } = new List<AnimalCard>();
        public List<Player> Players { get; set; } = new List<Player>();
        private Player user;

        public Player User
        {
            get { return user; }
            set { user = value; OnPropertyChanged?.Invoke(CurrentPlayer); }
        }
        private Player currentPlayer;

        public Player CurrentPlayer
        {
            get { return currentPlayer; }
            set { currentPlayer = value; OnPropertyChanged?.Invoke(CurrentPlayer); }
        }
        public Timer InactivityTimer { get; set; }

        public void TestMessage()
        {
            System.Diagnostics.Debug.WriteLine("GameStateTest");
        }

        public void GameSetup(string userName, List<string> playerNames, string startPlayer)
        {
            SetupPlayers(playerNames);
            CurrentPlayer = Players.First(player => player.Name == startPlayer);
            User = Players.First(player => player.Name == userName);
            System.Diagnostics.Debug.WriteLine(User.Name + " " + CurrentPlayer.Name);
            System.Diagnostics.Debug.WriteLine(User == CurrentPlayer);
            SetupAuctionCards();
            OnPropertyChanged?.Invoke(CurrentPlayer);
        }

        private void SetupAuctionCards()
        {
            foreach (AnimalTypes animalType in Enum.GetValues(typeof(AnimalTypes)))
            {
                for (int x = 0; x < 4; x++)
                {
                    RemainingAuctionCards.Add(new AnimalCard(animalType, (int)animalType));
                }
            }
        }

        private void SetupPlayers(List<string> playerNames)
        {
            foreach (string playerName in playerNames)
            {
                System.Diagnostics.Debug.WriteLine(playerName);
                Player player = new Player(playerName)
                {
                    MoneyCards = new List<int>() { 0, 0, 10, 10, 10, 10, 50 }
                };
                Players.Add(player);
            }
        }




    }
}
