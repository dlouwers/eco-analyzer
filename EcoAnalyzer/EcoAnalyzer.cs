/**
 * File: EcoAnalyzer.cs
 * Eco Version: 7.3
 * Mod Version: 0.1
 * 
 * Author: Dirk Louwers
 * 
 * 
 * This mod allows a server admin to send game event data to an external system for analysis
 * 
 * 
 * Features:
 * 
 */

namespace EcoAnalyzer
{
    using System;
    using System.Threading.Tasks;
    using Eco.Core.Plugins.Interfaces;
    using Eco.Gameplay.Stats;
    using Eco.Gameplay.Stats.ConcretePlayerActions;
    using Eco.Gameplay.Systems.Chat;
    using Eco.Shared.Services;

    public class EcoAnalyzer : IModKitPlugin, IThreadedPlugin
    {
        //private readonly static String analyzerHost = "ecoanalyzer.stormlantern.nl";
        //private readonly static int analyzerPort = 5000;
        private readonly static String analyzerHost = "www.nu.nl";
        private readonly static int analyzerPort = 443;

        public string GetStatus()
        {
            return String.Empty;
        }

        private void HandleCraftAction(CraftAction ca)
        {
            DateTime now = DateTime.Now;
            ChatManager.ServerMessageToAllLoc(EcoAnalyzerProtocol1.CraftMessage(now, ca), false, category: ChatCategory.Info);
        }

        private void HandleBuyAction(BuyAction ba)
        {
            DateTime now = DateTime.Now;
            ChatManager.ServerMessageToAllLoc(EcoAnalyzerProtocol1.BuyMessage(now, ba), false, category: ChatCategory.Info);
        }

        private void HandleSellAction(SellAction sa)
        {
            DateTime now = DateTime.Now;
            ChatManager.ServerMessageToAllLoc(EcoAnalyzerProtocol1.SellMessage(now, sa), false, category: ChatCategory.Info);
        }

        private void HandleHarvestAction(HarvestAction ha)
        {
            DateTime now = DateTime.Now;
            ChatManager.ServerMessageToAllLoc(EcoAnalyzerProtocol1.HarvestMessage(now, ha), false, category: ChatCategory.Info);
        }

        public async void Run()
        {
            // Connect to server
            try
            {
                EcoAnalyzerMessenger messenger = new EcoAnalyzerMessenger(analyzerHost, analyzerPort);
                await messenger.Start();
            }
            catch (Exception e)
            {
                ChatManager.ServerMessageToAllLoc($"EcoAnalyzer failed to connect: {e}", false);
            }
            // Subscribe to the events 
            PlayerActions.Craft.OnActionPerformed.Add(HandleCraftAction);
            PlayerActions.Buy.OnActionPerformed.Add(HandleBuyAction);
            PlayerActions.Sell.OnActionPerformed.Add(HandleSellAction);
            PlayerActions.Harvest.OnActionPerformed.Add(HandleHarvestAction);
            ChatManager.ServerMessageToAllLoc("EcoAnalyzer ready", false);
        }
    }
}
