using KnaagdierenMarktGame.Client.Enums;
using KnaagdierenMarktGame.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KnaagdierenMarktGame.Client.Classes
{
    public class GameState
    {
        public delegate Task PropertyChanged(object message);

        public event PropertyChanged OnPropertyChanged;

        public ObservableCollection<AnimalCard> RemainingAuctionCards { get; set; } = new ObservableCollection<AnimalCard>();

        //public ObservableCollection<AnimalCard> RemainingAuctionCards { get; set; } = new ObservableCollection<AnimalCard>();
        public List<Player> Players { get; set; } = new List<Player>();
        public List<string> PlayerOrder { get; set; } = new List<string>();

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

        private States currentState = States.None;

        public States CurrentState
        {
            get { return currentState; }
            set { currentState = value; OnPropertyChanged?.Invoke(CurrentState); }
        }

        //public States CurrentState { get; set; } = States.None;
        public Timer InactivityTimer { get; set; }

        public void TestMessage()
        {
            System.Diagnostics.Debug.WriteLine("GameStateTest");
        }

        private readonly GameConnection gameConnection;

        public GameState(GameConnection _gameConnection)
        {
            gameConnection = _gameConnection;
        }

        public void GameSetup(string userName, List<string> playerNames, string startPlayer)
        {
            RemainingAuctionCards.CollectionChanged += (object sender, NotifyCollectionChangedEventArgs e) => OnPropertyChanged?.Invoke(RemainingAuctionCards);
            SetupPlayers(playerNames);
            CurrentPlayer = Players.First(player => player.Name == startPlayer);
            User = Players.First(player => player.Name == userName);
            SetupAuctionCards();
            PlayerOrder.Add(CurrentPlayer.Name);
            //OnPropertyChanged?.Invoke(CurrentPlayer);
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

        public async Task NextPlayer()
        {
            Message message = new Message() { MessageType = MessageType.NextPlayer, Sender = User.Name };
            if (Players.Any(player => !PlayerOrder.Contains(player.Name)))
            {
                List<Player> NotChosenPlayers = Players.Where(player => !PlayerOrder.Contains(player.Name)).ToList();
                Random random = new Random();
                Player chosenPlayer = NotChosenPlayers[random.Next(0, NotChosenPlayers.Count())];
                message.Objects.Add(chosenPlayer.Name);
            }
            else
            {
                int index = PlayerOrder.FindIndex(player => player == CurrentPlayer.Name);
                message.Objects.Add(PlayerOrder[index == PlayerOrder.Count() - 1 ? 0 : index + 1]);
            }
            await gameConnection.HubConnection.SendAsync("SendMessage", message);
        }
    }
}
