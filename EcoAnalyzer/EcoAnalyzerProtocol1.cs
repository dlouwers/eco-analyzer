using Eco.Gameplay.Components;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Stats.ConcretePlayerActions;
using System;
using System.Linq;

namespace EcoAnalyzer
{
    static class EcoAnalyzerProtocol1
    {
        public static string CraftMessage(DateTime timestamp, CraftAction a)
        {
            return $"Craft(timestamp={timestamp:u}, worldtime={a.TimeSeconds}, item={a.ItemTypeName}, user={a.Username})\n";
        }

        public static string BuyMessage(DateTime timestamp, BuyAction a)
        {
            string currency = WorldObjectManager.GetFromID(a.WorldObjectId).GetComponent<CreditComponent>().CurrencyName;
            TradeOffer offer = (from TradeOffer o in WorldObjectManager.GetFromID(a.WorldObjectId).GetComponent<StoreComponent>().SellOffers()
                        where o.Stack.Item.FriendlyName == a.ItemTypeName
                        select o).FirstOrDefault();
            return $"Buy(timestamp={timestamp:u}, worldtime={a.TimeSeconds}, item={a.ItemTypeName}, price={offer.Price}, currency={currency}, user={a.Username})\n";
        }

        public static string SellMessage(DateTime timestamp, SellAction a)
        {
            string currency = WorldObjectManager.GetFromID(a.WorldObjectId).GetComponent<CreditComponent>().CurrencyName;
            TradeOffer offer = (from TradeOffer o in WorldObjectManager.GetFromID(a.WorldObjectId).GetComponent<StoreComponent>().SellOffers()
                                where o.Stack.Item.FriendlyName == a.ItemTypeName
                                select o).FirstOrDefault();
            return $"Sell(timestamp={timestamp:u}, worldtime={a.TimeSeconds}, item={a.ItemTypeName}, price={offer.Price}, currency={currency}, user={a.Username})\n";
        }

        internal static string HarvestMessage(DateTime timestamp, HarvestAction a)
        {
            return $"Harvest(timestamp={timestamp:u}, worldtime={a.TimeSeconds}, item={a.SpeciesName}, user={a.Username})\n";
        }
    }
}
