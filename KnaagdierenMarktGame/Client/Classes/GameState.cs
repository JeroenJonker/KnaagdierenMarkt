using KnaagdierenMarktGame.Client.Enums;
using KnaagdierenMarktGame.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using KnaagdierenMarktGame.Shared.Enums;

namespace KnaagdierenMarktGame.Client.Classes
{
    public class GameState
    {
        public delegate Task PropertyChanged(object message);

        public event PropertyChanged OnPropertyChanged;

        public ObservableCollection<AnimalCard> RemainingAuctionCards { get; set; } = new ObservableCollection<AnimalCard>();

        private List<int> StartingMoneyCards => new List<int>() { 0, 0, 10, 10, 10, 10, 50 };

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

        private States currentState = States.Login;

        public States CurrentState
        {
            get { return currentState; }
            set { currentState = value; OnPropertyChanged?.Invoke(CurrentState);  }
        }

        public Timer Timer { get; set; }

        private readonly PeerConnection messageReceiver;

        public GameState(ServerConnection _gameConnection, PeerConnection _messageReceiver)
        {
            messageReceiver = _messageReceiver;
            messageReceiver.OnNewGameMessage += HandleMessage;
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

        public void GameSetup(string userName, List<Player> playerNames, List<Player> playerOrder)
        {
            RemainingAuctionCards.CollectionChanged += (object sender, NotifyCollectionChangedEventArgs e) => OnPropertyChanged?.Invoke(RemainingAuctionCards);
            SetupPlayers(playerOrder);
            CurrentPlayer = Players.First(player => player.Name == playerOrder.First().Name);
            User = Players.First(player => player.Name == userName);
            messageReceiver.StartConnections(Players.Where(player => player.Name != user.Name).ToList());
            SetupAuctionCards();
            CurrentState = States.ChooseAction;
            //StartConnectionChecker();
        }

        public void StartConnectionChecker()
        {
            Timer = new Timer(3000);
            Timer.Elapsed += HandleTimer;
            Timer.Start();
        }
        //mogelijk herzien
        private async void HandleTimer(object sender, ElapsedEventArgs e)
        {
            RaiseAmountOfConnectionWarningsForPlayers();
            Message message = new Message() { MessageType = MessageType.StillConnected, Sender = User.Name };
            await messageReceiver.SendMessageToPeers(message);
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
                CurrentState = States.ChooseAction;
                Players.Remove(player);
                foreach (AnimalCard card in player.AnimalCards)
                {
                    RemainingAuctionCards.Add(card);
                }
                AddCompensationMoneyCardsToPlayers();
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
                //temporary
                if (animalType != AnimalTypes.Ezel)
                {
                    continue;
                }
                for (int x = 0; x < 4; x++)
                {
                    RemainingAuctionCards.Add(new AnimalCard(animalType, (int)animalType));
                }
            }
        }

        private void SetupPlayers(List<Player> players)
        {
            foreach (Player player in players)
            {
                player.MoneyCards = new List<int>(StartingMoneyCards);
                //player.AnimalCards.Add(new AnimalCard(AnimalTypes.Bok, 500)); /// wegahlen testen
                Players.Add(player);
            }
        }

        public async Task SendNextPlayerMessage()
        {
            Message message = new Message() { MessageType = MessageType.NextPlayer, Sender = User.Name };
            message.Objects.Add(GetNextPlayerName());
            await messageReceiver.SendMessageToPeers(message);
        }

        private string GetNextPlayerName()
        {
            int index = Players.FindIndex(player => player.Name == CurrentPlayer.Name);
            return Players[index == Players.Count() - 1 ? 0 : index + 1].Name;
        }
    }
}
