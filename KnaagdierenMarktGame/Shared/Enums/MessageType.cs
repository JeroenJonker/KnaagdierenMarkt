namespace KnaagdierenMarktGame.Shared
{
    public enum MessageType
    {
        JoinedGroup, NewGroup, InitGroups, LeftGroup, GroupDeleted, StartGame, NextPlayer, Auction, BuyOver, ChooseAction,
        PulledAuctionCard, BeginCountdown, MakeAuctionOffer, AcceptAuctionOffer, RightOfSale, FailedOffer, PayOffer,
        BuyOverCard, BuyOverOffer, BuyOverResult, GameEnd,
        StillConnected
    }
}
