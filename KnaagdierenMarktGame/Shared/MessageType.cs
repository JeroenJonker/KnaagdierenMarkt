using System;
using System.Collections.Generic;
using System.Text;

namespace KnaagdierenMarktGame.Shared
{
    public enum MessageType
    {
        GroupChanged, InitGroups, LeavedGroup, GroupDeleted, StartGame, NextPlayer, Auction, Trade, None,
        PulledAuctionCard, BeginCountdown, MakeAuctionOffer, AcceptAuctionOffer, RightOfSale, FailedOffer, PayOffer,
        BuyOutCard, BuyOutOffer, BuyOutResult,
        StillConnected
    }
}
