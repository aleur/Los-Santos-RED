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

public class TeleportInteract : InteriorInteract
{
    public int FadeTime { get; set; } = 1500;
    public SpawnPlace TeleportPlace { get; set; } 
    public TeleportInteract()
    {

    }

    public TeleportInteract(string name, Vector3 position, float heading, string buttonPromptText) : base(name, position, heading, buttonPromptText)
    {

    }

    public override void OnInteract()
    {
        if(TeleportPlace == null)
        {
            Game.DisplayHelp("Error Teleporting");
            return;
        }

        Interior.IsMenuInteracting = true;
        Interior?.RemoveButtonPrompts();
        RemovePrompt();
        MoveToPositionWithoutWaiting(1.0f);


        if (FadeTime > 0)
        {
            Game.FadeScreenOut(FadeTime, true);
        }


        Player.Character.Position = TeleportPlace.Position;
        Player.Character.Heading = TeleportPlace.Heading;
        GameFiber.Sleep(500);


        if (FadeTime > 0)
        {
            Game.FadeScreenIn(FadeTime, true);
        }


        Interior.IsMenuInteracting = false;
        LocationCamera?.ReturnToGameplay(true);
        LocationCamera?.StopImmediately(true);
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

