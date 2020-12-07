using KnaagdierenMarktGame.Client.Enums;
using KnaagdierenMarktGame.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
//using System.Threading;
using System.Threading.Tasks;
using System.Timers;

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
        public Timer Timer { get; set; }

        public void TestMessage()
        {
            System.Diagnostics.Debug.WriteLine("GameStateTest");
        }

        private readonly GameConnection gameConnection;

        public GameState(GameConnection _gameConnection)
        {
            gameConnection = _gameConnection;
            gameConnection.OnNewGameMessage += HandleMessage;
        }

        private Task HandleMessage(Message message)
        {
            if (message.MessageType == MessageType.StillConnected)
            {
                ResetAmountOfConnectionWarningsForPlayer(message.Sender);
            }
            return null;
        }

        private void ResetAmountOfConnectionWarningsForPlayer(string playername) => 
            Players.First(player => player.Name == playername).AmountOfConnectionWarnings = 0;

        public void GameSetup(string userName, List<string> playerNames, string startPlayer)
        {
            RemainingAuctionCards.CollectionChanged += (object sender, NotifyCollectionChangedEventArgs e) => OnPropertyChanged?.Invoke(RemainingAuctionCards);
            SetupPlayers(playerNames);
            CurrentPlayer = Players.First(player => player.Name == startPlayer);
            User = Players.First(player => player.Name == userName);
            SetupAuctionCards();
            PlayerOrder.Add(CurrentPlayer.Name);
            StartConnectionChecker();
            //OnPropertyChanged?.Invoke(CurrentPlayer);
        }

        public void StartConnectionChecker()
        {
            Timer = new Timer(5000);
            Timer.Elapsed += HandleTimer;
            Timer.Start();
        }

        private async void HandleTimer(object sender, ElapsedEventArgs e)
        {
            RaiseAmountOfConnectionWarningsForPlayers();
            Message message = new Message() { MessageType = MessageType.StillConnected, Sender = User.Name };
            await gameConnection.HubConnection.SendAsync("SendMessage", message);
        }

        private void RaiseAmountOfConnectionWarningsForPlayers()
        {
            foreach (Player player in Players)
            {
                player.AmountOfConnectionWarnings++;
                if (player.AmountOfConnectionWarnings > 2)
                {
                    KickPlayer(player);
                }
            }
        }

        private void KickPlayer(Player player)
        {
            if (Players.Count > 1)
            {
                DetermineNewCurrentPlayer(player);
                CurrentState = States.None;
                Players.Remove(player);
                PlayerOrder.Remove(player.Name);
                foreach (AnimalCard card in player.AnimalCards)
                {
                    RemainingAuctionCards.Add(card);
                }
                AddCompensationMoneyCardsToPlayers();
            }
            else
            {
                // weet nog niet D:
            }
        }

        private void DetermineNewCurrentPlayer(Player player)
        {
            if (CurrentPlayer == player)
            {
                CurrentPlayer = Players.First(nextplayer => nextplayer.Name == GetNextPlayerName());
            }
        }

        private void AddCompensationMoneyCardsToPlayers()
        {
            foreach (Player player in Players)
            {
                player.MoneyCards.AddRange(new List<int>{ 0, 10, 10});
            }
        }

        public List<int> SubtractListFromList(List<int> listA, List<int> listB)
        {
            foreach (int number in listB)
            {
                if (listA.Contains(number))
                {
                    listA.Remove(number);
                }
            }
            return listA;
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
                    MoneyCards = new List<int>(StartingMoneyCards)
                };
                Players.Add(player);
            }
        }

        private List<int> StartingMoneyCards => new List<int>() { 0, 0, 10, 10, 10, 10, 50 };

        public async Task SendNextPlayerMessage()
        {
            Message message = new Message() { MessageType = MessageType.NextPlayer, Sender = User.Name };
            message.Objects.Add(GetNextPlayerName());
            await gameConnection.HubConnection.SendAsync("SendMessage", message);
        }

        private string GetNextPlayerName()
        {
            if (Players.Any(player => !PlayerOrder.Contains(player.Name)))
            {
                List<Player> NotChosenPlayers = Players.Where(player => !PlayerOrder.Contains(player.Name)).ToList();
                Random random = new Random();
                Player chosenPlayer = NotChosenPlayers[random.Next(0, NotChosenPlayers.Count())];
                return chosenPlayer.Name;
            }
            else
            {
                int index = PlayerOrder.FindIndex(player => player == CurrentPlayer.Name);
                return PlayerOrder[index == PlayerOrder.Count() - 1 ? 0 : index + 1];
            }
        }
    }
}
