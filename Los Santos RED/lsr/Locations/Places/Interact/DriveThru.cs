﻿using LosSantosRED.lsr.Interface;
using Rage;
using Rage.Native;
using RAGENativeUI.Elements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

public class DriveThru : InteractableLocation
{
    private LocationCamera StoreCamera;
    private IActivityPerformable Player;
    private IModItems ModItems;
    private IEntityProvideable World;
    private ISettingsProvideable Settings;
    private IWeapons Weapons;
    private ITimeControllable Time;
    private Transaction Transaction;
    public DriveThru() : base()
    {

    }
    public override int MapIcon { get; set; } = 523;
    public override Color MapIconColor { get; set; } = Color.White;
    public override float MapIconScale { get; set; } = 1.0f;
    public override string ButtonPromptText { get; set; }
    public DriveThru(Vector3 _EntrancePosition, float _EntranceHeading, string _Name, string _Description, string menuID) : base(_EntrancePosition, _EntranceHeading, _Name, _Description)
    {
        MenuID = menuID;
        ButtonPromptText = $"Shop at {Name}";
    }
    public override void OnInteract(ILocationInteractable player, IModItems modItems, IEntityProvideable world, ISettingsProvideable settings, IWeapons weapons, ITimeControllable time)
    {
        Player = player;
        ModItems = modItems;
        World = world;
        Settings = settings;
        Weapons = weapons;
        Time = time;

        if (CanInteract)
        {
            Player.IsInteractingWithLocation = true;
            CanInteract = false;

            GameFiber.StartNew(delegate
            {
                //StoreCamera = new LocationCamera(this, Player);
                // StoreCamera.Setup();

                NativeFunction.Natives.SET_GAMEPLAY_COORD_HINT(EntrancePosition.X, EntrancePosition.Y, EntrancePosition.Z, -1, 2000, 2000);



                CreateInteractionMenu();
                Transaction = new Transaction(MenuPool, InteractionMenu, Menu, this);

                Transaction.PreviewItems = false;

                Transaction.CreateTransactionMenu(Player, modItems, world, settings, weapons, time);

                InteractionMenu.Visible = true;
                InteractionMenu.OnItemSelect += InteractionMenu_OnItemSelect;
                Transaction.ProcessTransactionMenu();

                Transaction.DisposeTransactionMenu();
                DisposeInteractionMenu();

                // StoreCamera.Dispose();

                NativeFunction.Natives.STOP_GAMEPLAY_HINT(false);


                Player.IsInteractingWithLocation = false;
                CanInteract = true;
            }, "DriveThruInteract");
        }
    }
    private void InteractionMenu_OnItemSelect(RAGENativeUI.UIMenu sender, UIMenuItem selectedItem, int index)
    {
        if (selectedItem.Text == "Buy")
        {
            Transaction?.SellMenu?.Dispose();
            Transaction?.PurchaseMenu?.Show();
        }
        else if (selectedItem.Text == "Sell")
        {
            Transaction?.PurchaseMenu?.Dispose();
            Transaction?.SellMenu?.Show();
        }
    }

}
