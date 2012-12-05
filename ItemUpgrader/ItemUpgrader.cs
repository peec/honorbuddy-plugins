using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Styx.Plugins;
using Styx.WoWInternals;
using Styx.CommonBot;
using Styx.WoWInternals.WoWObjects;
using Styx;
using Styx.CommonBot.Profiles;
using Styx.Common;
using Styx.CommonBot.POI;
using Styx.Common.Helpers;
using System.Drawing;
using System.Threading;
using Styx.CommonBot.Database;
using WindowsFormsApplication1;




/*
 * Lua functions:
 * ClearItemUpgrade
 * CloseItemUpgrade
 * GetItemUpdateLevel
 * GetItemUpgradeItemInfo
 * GetItemUpgradeStats
 * SetItemUpgradeFromCursorItem
 * 
 * Events:
 * ITEM_UPGRADE_MASTER_CLOSED
 * ITEM_UPGRADE_MASTER_OPENED
 * ITEM_UPGRADE_MASTER_SET_ITEM
 * ITEM_UPGRADE_MASTER_UPDATE
 * 
 * 
 **/
namespace com.peec.itemupgrader
{
    class ItemUpgrader : HBPlugin
    {
        public override string Name { get { return "ItemUpgrader V. " + Version; } }
        public override string Author { get { return "Peec"; } }
        public override Version Version { get { return new Version(0, 0, 5); } }
        public override bool WantButton { get { return true; } }
        public override string ButtonText { get { return "ItemUpgrader Settings"; } }
        public LocalPlayer Me { get { return Styx.StyxWoW.Me; } }
        public override void OnButtonPress()
        {
            new FormSettings().ShowDialog();
        }

        const int UPGRADE_PRICE_HONOR = 1500;
        const int UPGRADE_PRICE_JUSTICE = 1500;
        const int UPGRADE_PRICE_CONQUEST = 750;
        const int UPGRADE_PRICE_VALOR = 750;
        const int MIN_ITEM_LEVEL = 458;

        uint[] ID_HORDE_VENDORS = { 68979, 68981};
        uint[] ID_ALLIANCE_VENDORS = {68980, 68982};


        public List<ulong> blacklistedItems = new List<ulong>();


        WoWItem currentItem; // Current item checked.
        WoWCurrency currentHonor, currentConquest, currentJustice, currentValor;
        private WaitTimer lockTimer = new WaitTimer(TimeSpan.FromSeconds(2));
        WoWItem[] items;
        private bool currentlyCheckingItemUpgrades = false; // flag for currently in vendor.

        
        bool errorNotNearVendorExplained = false;

        Dictionary<string, LuaEventHandlerDelegate> luaEvents = new Dictionary<string, LuaEventHandlerDelegate>();

        public override void Initialize()
        {
            SLog("Starting ItemUpgrader.");

            luaEvents.Add("ITEM_UPGRADE_MASTER_OPENED", OnOpenVendor);
            luaEvents.Add("ITEM_UPGRADE_MASTER_SET_ITEM", OnSetItemUpgrade);
            luaEvents.Add("ITEM_UPGRADE_MASTER_UPDATE", OnUpgradeItem);
            luaEvents.Add("ITEM_UPGRADE_MASTER_CLOSED", OnVendorClose);
            
            foreach (KeyValuePair<string, LuaEventHandlerDelegate> pair in luaEvents)
            {
                SDebug("Attaching Lua Event: " + pair.Key);
                Lua.Events.AttachEvent(pair.Key, pair.Value);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            errorNotNearVendorExplained = false;
            items = null;
            currentItem = null;
            blacklistedItems = new List<ulong>();

            foreach (KeyValuePair<string, LuaEventHandlerDelegate> pair in luaEvents)
            {
                SDebug("Detach lua event {0}", pair.Key);
                Lua.Events.DetachEvent(pair.Key, pair.Value);
            }

        }


        public override void Pulse()
        {

            if (!lockTimer.IsFinished)
                return;
            lockTimer.Reset(); // Lock pulse for a while. Deal with lua events.



            // Simple stop pulse.
            if (
                currentlyCheckingItemUpgrades || // currentlyCheckingItemUpgrades is important lock.
                !Me.IsAlive || // Skip if not alive.
                (Battlegrounds.IsInsideBattleground || Me.IsInInstance)) // Skip if in bg / instance.
            {
                return;
            }



            WoWUnit vendorunit = null;
            uint[] vendorIds = (Me.IsAlliance ? ID_ALLIANCE_VENDORS : ID_HORDE_VENDORS);
            foreach (uint vendorId in vendorIds)
            {
                vendorunit = ObjectManager.GetObjectsOfType<WoWUnit>(false, false).FirstOrDefault(
                    u => u.Entry == vendorId
                );
                if (vendorunit != null) continue;
            }
            
            
            if (vendorunit == null)
            {
                if (!errorNotNearVendorExplained)
                {
                    errorNotNearVendorExplained = true;
                    SError("Not buying item upgrades, you are not near {0} NPC ID: #{1} . See http://www.wowhead.com/npc={1} for location.", (Me.IsAlliance ? "Alliance" : "Horde"), vendorIds[0]);
                }
                return;
            }


            if ((vendorunit != null && !vendorunit.WithinInteractRange))
            {
                Styx.Pathing.Navigator.MoveTo(vendorunit.Location);
                return;
            }


            // Cache some values 
            items = Me.Inventory.Equipped.Items;
            // Order by highest ilevel first, since thats the items we want to upgrade first.
            items = items.OrderByDescending(item => item != null ? item.ItemInfo.Level : 0).ToArray();
            currentHonor = WoWCurrency.GetCurrencyByType(WoWCurrencyType.HonorPoints);
            currentConquest = WoWCurrency.GetCurrencyByType(WoWCurrencyType.HonorPoints);
            currentJustice = WoWCurrency.GetCurrencyByType(WoWCurrencyType.JusticePoints);
            currentValor = WoWCurrency.GetCurrencyByType(WoWCurrencyType.ValorPoints);


            uint previousItem = currentItem == null ? 0 : currentItem.ItemInfo.Id;
            currentItem = getBuyItemUpgradeOnItem();

            if (currentItem == null)
            {
                return;
            }
            

            vendorunit.Interact();

        }


        /**
         * Returns null if no item to upgrade based on stats / currency amount.
         **/
        public WoWItem getBuyItemUpgradeOnItem()
        {
            foreach (WoWItem item in items)
            {
                if (item == null) continue;

                if (
                    item.ItemInfo.Level >= MIN_ITEM_LEVEL &&
                    !blacklistedItems.Contains(item.Guid) &&
                    (item.Quality == WoWItemQuality.Epic || item.Quality == WoWItemQuality.Rare))
                {

                    if (item.IsPvPItem)
                    {
                        switch (item.Quality)
                        {
                            case WoWItemQuality.Rare:
                                if (!ItemUpgraderSettings.Instance.enableHonor) continue;
                                if (currentHonor.Amount < UPGRADE_PRICE_HONOR) continue;
                                break;
                            case WoWItemQuality.Epic:
                                if (!ItemUpgraderSettings.Instance.enableConquest) continue;
                                if (currentConquest.Amount < UPGRADE_PRICE_CONQUEST) continue;
                                break;
                        }
                    }
                    else
                    {
                        switch (item.Quality)
                        {
                            case WoWItemQuality.Rare:
                                if (!ItemUpgraderSettings.Instance.enableJustice) continue;
                                if (currentJustice.Amount < UPGRADE_PRICE_JUSTICE) continue;
                                break;
                            case WoWItemQuality.Epic:
                                if (!ItemUpgraderSettings.Instance.enableValor) continue;
                                if (currentValor.Amount < UPGRADE_PRICE_VALOR) continue;
                                break;
                        }
                    }

                    return item;
                }
            }

            SDebug("No items to upgrade at this time due to lack of points or all items upgraded. Awaiting for points / more gear.");

            return null;
        }



        #region Lua Events
        public void OnOpenVendor(object sender, LuaEventArgs args)
        {
            currentlyCheckingItemUpgrades = true;
            SDebug("Event: InVendor, Trying to check {0} for upgrades..", currentItem.Name);

            SLog("We got new item to upgrade: {0}. You have enough points + correct gear requirement. Starting procedure.", currentItem.Name);

            currentItem.PickUp();

            Lua.DoString("SetItemUpgradeFromCursorItem()");


        }
        public void OnVendorClose(object sender, LuaEventArgs args)
        {
            currentlyCheckingItemUpgrades = false;
        }


        /**
         * /run local icon, name, quality, bound, numCurrUpgrades, numMaxUpgrades, cost, currencyType = GetItemUpgradeItemInfo()
         **/
        public void OnSetItemUpgrade(object sender, LuaEventArgs args)
        {

            List<string> values = Lua.GetReturnValues("return GetItemUpgradeItemInfo()");
            int upgradeAmount = Convert.ToInt32(values[4]);
            int maxAmount = Convert.ToInt32(values[5]);
            uint cost = Convert.ToUInt32(values[6]);
            uint currencyType = Convert.ToUInt32(values[7]);



            SDebug("Event: OnSetItemUpgrade, On Item {4}, amount: {0}, maxAmount: {1}, cost: {2}, currencyType: {3}",
                upgradeAmount,
                maxAmount,
                cost,
                currencyType,
                currentItem.Name);

            if (upgradeAmount < maxAmount)
            {

                Lua.DoString("UpgradeItem()");

            }
            else
            {
                blacklistedItems.Add(currentItem.Guid);

                SLog("{0} is fully upgraded, blacklisting item for upgrades. ", currentItem.Name);

                Lua.DoString("CloseItemUpgrade()");
            }


        }

        public void OnUpgradeItem(object sender, LuaEventArgs args)
        {

            SDebug("Event: OnUpgradeItem.");

            SLog("Upgraded item {0}. ", currentItem.Name);

            Lua.DoString("CloseItemUpgrade()");
        }
        #endregion



        #region Helpers
        public static void SDebug(string format, params object[] args)
        {
            if (ItemUpgraderSettings.Instance.enableDebug) Logging.Write("[ItemUpgrader:Debug]: " + format, args);
        }
        public static void SLog(string format, params object[] args)
        {
            Logging.Write("[ItemUpgrader]: " + format, args);
        }
        public static void SError(string format, params object[] args)
        {
            Logging.Write("[ItemUpgrader]: " + format, args);
        }
        #endregion
    }
}
