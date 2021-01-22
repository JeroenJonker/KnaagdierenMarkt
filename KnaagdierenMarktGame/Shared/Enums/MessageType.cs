namespace KnaagdierenMarktGame.Shared
{
    public enum MessageType
    {
        JoinedGroup, NewGroup, InitGroups, LeftGroup, GroupDeleted, StartGame, NextPlayer, Auction, Trade, None,
        PulledAuctionCard, BeginCountdown, MakeAuctionOffer, AcceptAuctionOffer, RightOfSale, FailedOffer, PayOffer,
        BuyOutCard, BuyOutOffer, BuyOutResult,
        StillConnected
    }
}
