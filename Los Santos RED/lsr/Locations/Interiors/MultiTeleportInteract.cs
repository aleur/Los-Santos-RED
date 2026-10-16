using Rage.Native;
using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LosSantosRED.lsr.Interface;
using System.Xml.Serialization;
using RAGENativeUI;
using LosSantosRED.lsr;
using ExtensionsMethods;
using RAGENativeUI.Elements;

public class MultiTeleportInteract : InteriorInteract
{
    public int FadeTime { get; set; } = 1500;
   
    public MultiTeleportInteract()
    {

    }

    public MultiTeleportInteract(string name, Vector3 position, float heading, string buttonPromptText) : base(name, position, heading, buttonPromptText)
    {

    }

    public override void OnInteract()
    {
        Interior.IsMenuInteracting = true;
        Interior?.RemoveButtonPrompts();
        RemovePrompt();

        //SetupCamera(false);
        //if (!WithWarp)
        //{
        //    if (!MoveToPosition())
        //    {
        //        Interior.IsMenuInteracting = false;
        //        Game.DisplayHelp("Interact Failed");
        //        LocationCamera?.StopImmediately(true);
        //        return;
        //    }
        //}

        MoveToPositionWithoutWaiting(1.0f);

        ShowTeleportMenu();
    }

    private void ShowTeleportMenu()
    {
        if(Interior == null)
        {
            EntryPoint.WriteToConsole("MultiTeleportInteract: NO INTERIOR FOUND");
            return;
        }
        List<InteriorInteract> interacts = Interior.InteractPoints.Where(x => x.GroupName == GroupName && x.Position != Position).ToList();
        List<SpawnPlace> TeleportPlaces = new List<SpawnPlace>();
        foreach (InteriorInteract interact in interacts)
        {
            TeleportPlaces.Add(new SpawnPlace(interact.Position, interact.Heading) { Name = interact.Name });
        }


        if(!TeleportPlaces.Any())
        {
            Interior.IsMenuInteracting = false;
            //LocationCamera?.ReturnToGameplay(true);
            //LocationCamera?.StopImmediately(true);
            EntryPoint.WriteToConsole("MultiTeleportInteract: NO TELEPORT PLACES FOUND");
            return;
        }


        MenuPool teleportMenuPool = new MenuPool();
        UIMenu uIMenu = new UIMenu("Destination", "Select destination");

        teleportMenuPool.Add(uIMenu);

        bool hasTeleported = false;
        foreach (SpawnPlace place in TeleportPlaces)
        {
            UIMenuItem uIMenuItem = new UIMenuItem(place.Name);
            uIMenuItem.Activated += (sender, e) =>
            {
                hasTeleported = true;
                uIMenu.Visible = false;
                TeleportToPlace(place);
            };
            uIMenu.AddItem(uIMenuItem);
        }
        uIMenu.Visible = true;

        while (EntryPoint.ModController.IsRunning && Player.IsAliveAndFree && teleportMenuPool.IsAnyMenuOpen())
        {
            teleportMenuPool.ProcessMenus();
            GameFiber.Yield();
        }
        if (!hasTeleported)
        {
            Interior.IsMenuInteracting = false;
            //LocationCamera?.ReturnToGameplay(true);
            //LocationCamera?.StopImmediately(true);
        }
        EntryPoint.WriteToConsole("MultiTeleportInteract: Interact Ended");
    }

    private void TeleportToPlace(SpawnPlace place)
    {
        if (FadeTime > 0)
        {
            Game.FadeScreenOut(FadeTime, true);
        }


        Player.Character.Position = place.Position;
        Player.Character.Heading = place.Heading;
        GameFiber.Sleep(500);


        if (FadeTime > 0)
        {
            Game.FadeScreenIn(FadeTime, true);
        }

        Interior.IsMenuInteracting = false;
        //LocationCamera?.ReturnToGameplay(true);
        //LocationCamera?.StopImmediately(true);
    }
    public override void AddPrompt()
    {
        if (Player == null)
        {
            return;
        }
        Player.ButtonPrompts.AttemptAddPrompt(Name, ButtonPromptText, Name, Settings.SettingsManager.KeySettings.InteractStart, 999);
    }
}

