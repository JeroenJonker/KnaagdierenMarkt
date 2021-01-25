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
using Newtonsoft.Json;

namespace KnaagdierenMarktGame.Client.Classes
{
    public class GameState
    {
        public delegate Task PropertyChanged(object message);

        public event PropertyChanged OnPropertyChanged;

        public ObservableCollection<AnimalTypes> RemainingAuctionCards { get; set; } = new ObservableCollection<AnimalTypes>();

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
            set { currentState = value; OnPropertyChanged?.Invoke(CurrentState); }
        }

        public Timer Timer { get; set; }

        private readonly PeerConnection peerConnection;

        public GameState(PeerConnection _peerConnection)
        {
            peerConnection = _peerConnection;
            peerConnection.OnNewGameMessage += HandleMessage;
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
            peerConnection.StartConnections(Players.Where(player => player.Name != user.Name).ToList());
            SetupAuctionCards();
            CurrentState = States.ChooseAction;
            StartConnectionChecker();
        }

        public void StartConnectionChecker()
        {
            Timer = new Timer(3000);
            Timer.Elapsed += HandleTimer;
            Timer.Start();
        }

        private async void HandleTimer(object sender, ElapsedEventArgs e)
        {
            RaiseAmountOfConnectionWarningsForPlayers();
            Message message = new Message() { MessageType = MessageType.StillConnected, Sender = User.Name };
            await peerConnection.SendMessageToPeers(message);
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
                foreach (AnimalTypes card in player.AnimalCards)
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
                player.MoneyCards.AddRange(new List<int> { 0, 10, 10 });
            }
        }

        public List<int> SubtractListBFromListA(List<int> listA, List<int> listB)
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
                if (animalType != AnimalTypes.Haan)
                {
                    continue;
                }
                for (int x = 0; x < 4; x++)
                {
                    RemainingAuctionCards.Add(animalType);
                }
            }
        }

        private void SetupPlayers(List<Player> players)
        {
            foreach (Player player in players)
            {
                player.MoneyCards = new List<int>(StartingMoneyCards);
                player.AnimalCards.Add(AnimalTypes.Bok); /// wegahlen testen
                Players.Add(player);
            }
        }

        public async Task SendNextPlayerMessage()
        {
            Message message = new Message() { Sender = User.Name };
            if (GetNextPlayerName() is string nextPlayer && !string.IsNullOrEmpty(nextPlayer))
            {
                message.MessageType = MessageType.NextPlayer;
                message.Objects.Add(nextPlayer);
            }
            else
            {
                message.MessageType = MessageType.GameEnd;
            }
            await peerConnection.SendMessageToPeers(message);
        }

        private string GetNextPlayerName()
        {
            int userIndex = Players.FindIndex(player => player.Name == CurrentPlayer.Name);
            int nextPlayerIndex = userIndex == Players.Count() - 1 ? 0 : userIndex + 1;
            return GetNextAvailablePlayer(userIndex, nextPlayerIndex);
        }
        private string GetNextAvailablePlayer(int userIndex, int nextPlayerIndex)
        {
            if (userIndex == nextPlayerIndex)
                return "";
            if (RemainingAuctionCards.Count() > 0 || CanPlayerTradeAnimalCards(Players[nextPlayerIndex]))
                return Players[nextPlayerIndex].Name;
            return GetNextAvailablePlayer(userIndex, nextPlayerIndex + 1 == Players.Count() ? 0 : nextPlayerIndex + 1);
        }

        public bool CanPlayerTradeAnimalCards(Player player) => Players.Where(pl => pl.Name != player.Name).Any(pl => pl.AnimalCards.Any(AnimalCard => HasPlayerAnimalCard(player, AnimalCard)));
        
        public bool HasPlayerAnimalCard(Player player, AnimalTypes animalCard) => player.AnimalCards.Any(animalcard => animalcard == animalCard);
        
        public List<int> ConvertObjectToListOfInt(object toBeConvertedObject)
        {
            if (toBeConvertedObject is List<int> list)
            {
                return list;
            }
            else
            {
                return JsonConvert.DeserializeObject<List<int>>(toBeConvertedObject.ToString());
            }
        }

        public AnimalTypes ConvertObjectToAnimalType(object toBeConvertedObject)
        {
            if (toBeConvertedObject is AnimalTypes)
            {
                return (AnimalTypes) Enum.Parse(typeof(AnimalTypes), toBeConvertedObject.ToString());
            }
            else
            {
                return JsonConvert.DeserializeObject<AnimalTypes>(toBeConvertedObject.ToString());
            }
        }

    }
}
