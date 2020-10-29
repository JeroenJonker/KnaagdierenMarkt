using System;
using System.Collections.Generic;
using System.Text;

namespace KnaagdierenMarktGame.Shared
{
    public enum MessageType
    {
        GroupChanged, InitGroups, LeavedGroup, GroupDeleted, StartGame,
        PulledAuctionCard, BeginCountdown, MakeAuctionOffer, AcceptAuctionOffer, RightOfSale, FailedOffer, PayOffer
    }
}
