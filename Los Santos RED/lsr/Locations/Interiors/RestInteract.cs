﻿using Rage.Native;
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

public class RestInteract : InteriorInteract
{
    private AnimationBundle endBundle;
    [XmlIgnore]
    public IRestableLocation RestableLocation {get;set;}
    public List<AnimationBundle> StartAnimations { get;set;} = new List<AnimationBundle>() { 
        //new AnimationBundle("savebighouse@", "f_getin_l_bighouse", (int)(eAnimationFlags.AF_HOLD_LAST_FRAME | eAnimationFlags.AF_TURN_OFF_COLLISION), 4.0f, -4.0f) { Gender = "F" },
        new AnimationBundle("savem_default@", "m_getin_l", (int)(eAnimationFlags.AF_HOLD_LAST_FRAME | eAnimationFlags.AF_TURN_OFF_COLLISION), 4.0f, -4.0f) { Gender = "U" } };
    public List<AnimationBundle> LoopAnimations { get; set; } = new List<AnimationBundle>() { 
        //new AnimationBundle("savebighouse@", "f_sleep_l_loop", (int)(eAnimationFlags.AF_LOOPING | eAnimationFlags.AF_TURN_OFF_COLLISION), 4.0f, -4.0f) { Gender = "F" },
        new AnimationBundle("savem_default@", "m_sleep_l_loop", (int)(eAnimationFlags.AF_LOOPING | eAnimationFlags.AF_TURN_OFF_COLLISION), 4.0f, -4.0f) { Gender = "U" } };
    public List<AnimationBundle> EndAnimations { get; set; } = new List<AnimationBundle>() { 
        //new AnimationBundle("savebighouse@", "f_getout_l_bighouse", (int)(eAnimationFlags.AF_HOLD_LAST_FRAME | eAnimationFlags.AF_TURN_OFF_COLLISION), 4.0f, -4.0f) { Gender = "F" },
        new AnimationBundle("savem_default@", "m_getout_l", (int)(eAnimationFlags.AF_HOLD_LAST_FRAME | eAnimationFlags.AF_TURN_OFF_COLLISION), 4.0f, -4.0f) { Gender = "U" } };
    public RestInteract()
    {
    }

    public RestInteract(string name, Vector3 position, float heading, string buttonPromptText) : base(name, position, heading, buttonPromptText)
    {

    }

    public override void OnInteract()
    {
        Interior.IsMenuInteracting = true;
        Interior?.RemoveButtonPrompts();
        RemovePrompt();
        SetupCamera();
        if (!WithWarp)
        {
            if (!MoveToPosition())
            {
                Interior.IsMenuInteracting = false;
                Game.DisplayHelp("Resting Failed");
                LocationCamera?.StopImmediately(true);
                return;
            }
        }
        if (!DoRestAnimation())
        {
            Interior.IsMenuInteracting = false;
            Game.DisplayHelp("Resting Failed");
            LocationCamera?.StopImmediately(true);
            return;
        }
        RestableLocation.CreateRestMenu();
        Interior.IsMenuInteracting = false;
        DoGetUpAnimation();
        LocationCamera?.ReturnToGameplay(true);
        LocationCamera?.StopImmediately(true);
    }
    public override void AddPrompt()
    {
        if (Player == null)
        {
            return;
        }
        Player.ButtonPrompts.AddPrompt(Name, ButtonPromptText, Name, Settings.SettingsManager.KeySettings.InteractStart, 999);
    }
    private bool DoRestAnimation()
    {
        Player.Character.Position = Position;
        Player.Character.Heading = Heading;
        HashSet<string> dictionaryList = new HashSet<string>();
        AnimationBundle startBundle = StartAnimations.Where(x => x.Gender == "U" || x.Gender == Player.Gender).PickRandom();
        if(startBundle != null)
        {
            dictionaryList.Add(startBundle.Dictionary);
        }
        AnimationBundle loopBundle = LoopAnimations.Where(x => x.Gender == "U" || x.Gender == Player.Gender).PickRandom();
        if (loopBundle != null)
        {
            dictionaryList.Add(loopBundle.Dictionary);
        }
        endBundle = EndAnimations.Where(x => x.Gender == "U" || x.Gender == Player.Gender).PickRandom();
        if (endBundle != null)
        {
            dictionaryList.Add(endBundle.Dictionary);
        }
        foreach(string dictionary in dictionaryList)
        {
            if (!AnimationDictionary.RequestAnimationDictionayResult(dictionary))
            {
                return false;
            }
        }
        if(startBundle != null)
        { 
            NativeFunction.Natives.TASK_PLAY_ANIM(Player.Character, startBundle.Dictionary, startBundle.Name, startBundle.BlendIn, startBundle.BlendOut, startBundle.Time, startBundle.Flags, 0, false, false, false);
            WaitForAnimation(endBundle.Dictionary, startBundle.Name);
        }
        if(loopBundle != null)
        { 
            NativeFunction.Natives.TASK_PLAY_ANIM(Player.Character, loopBundle.Dictionary, loopBundle.Name, loopBundle.BlendIn, loopBundle.BlendOut, loopBundle.Time, loopBundle.Flags, 0, false, false, false);
        }
        return true;
    }
    public bool DoGetUpAnimation()
    {
        if (endBundle != null)
        {
            NativeFunction.Natives.TASK_PLAY_ANIM(Player.Character, endBundle.Dictionary, endBundle.Name, endBundle.BlendIn, endBundle.BlendOut, endBundle.Time, endBundle.Flags, 0, false, false, false);
            WaitForAnimation(endBundle.Dictionary, endBundle.Name);
        }
        NativeFunction.Natives.CLEAR_PED_TASKS(Player.Character);
        return true;
    }

}
