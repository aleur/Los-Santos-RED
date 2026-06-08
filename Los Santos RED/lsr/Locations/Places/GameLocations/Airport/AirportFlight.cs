using LosSantosRED.lsr.Interface;
using Rage;
using Rage.Native;
using RAGENativeUI;
using RAGENativeUI.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
using System.Xml.Serialization;
[Serializable]
public class AirportFlight
{
    public string ToAirportID { get; set; }
    public string CarrierID { get; set; }




    public string ID { get; set; }
    public string Description { get; set; }
    public int Cost => (int)(CostPerMile * Distance);
    public int CostPerMile { get; set; }
    public int FlightTime { get; set; }
    public int HungerGain { get; set; }
    public int ThirstGain { get; set; }
    public int SleepGain { get; set; }
    [XmlIgnore]
    public double Distance { get; set; }
    [XmlIgnore]
    private string FormattedHunger => HungerGain >= 0 ? $"~g~{HungerGain}~s~" : $"~r~{HungerGain}~s~";
    [XmlIgnore]
    private string FormattedThirst => ThirstGain >= 0 ? $"~g~{ThirstGain}~s~" : $"~r~{ThirstGain}~s~";
    [XmlIgnore]
    private string FormattedSleep => SleepGain >= 0 ? $"~g~{SleepGain}~s~" : $"~r~{SleepGain}~s~";






    public AirportFlight()
    {

    }

    public AirportFlight(string airportID, string airline, string description, int costPerMile, int flightTime)
    {
        ToAirportID = airportID;
        CarrierID = airline;
        Description = description;
        CostPerMile = costPerMile;
        FlightTime = flightTime;
    }
    private string FormattedDescription(ILocationInteractable Player) => string.IsNullOrEmpty(Description) ? "" : Description + 
                                                                         $"~n~Hours: ~y~{FlightTime}~s~" +
                                                                         $"~n~Distance: ~p~{Distance}~s~ mi." +
                                                                         $"~n~~n~Hunger: {FormattedHunger}" +
                                                                         $"~n~Thirst: {FormattedThirst}" +
                                                                         $"~n~Sleep: {FormattedSleep}";
    public void AddToMenu(Airport airport, Airport destinationAirport, UIMenu rageMenu, ILocationInteractable player, IEntityProvideable World)
    {
        if (destinationAirport == null || airport == null || rageMenu == null || player == null) return;

        Distance = Math.Round(airport.EntrancePosition.DistanceTo2D(destinationAirport.EntrancePosition) * 0.000621371, 2);
        bool canFly = player.BankAccounts.GetMoney(true) >= Cost && destinationAirport.IsEnabled && (!destinationAirport.RequiresMPMap || World.IsMPMapLoaded);
        UIMenuItem uIMenuItem = new UIMenuItem($"{destinationAirport.Name} ({CarrierID})", FormattedDescription(player))
        {
            Enabled = canFly,
            RightLabel = player.BankAccounts.GetMoney(true) >= Cost ? $"~g~${Cost}~s~" : $"~r~${Cost}~s~"
        };

        uIMenuItem.Activated += (sender, selecteditem) =>
        {
            if (!selecteditem.Enabled)
            {
                // Messages don't work, but wahtever
                if (!destinationAirport.IsEnabled || (destinationAirport.RequiresMPMap && !World.IsMPMapLoaded))
                {
                    airport.PlayErrorSound();
                    airport.DisplayMessage("~r~Airport Closed", "We are sorry, the airport you are traveling to is closed.");
                }
                else if (player.BankAccounts.GetMoney(true) < Cost)
                {
                    airport.PlayErrorSound();
                    airport.DisplayMessage("~r~Insufficient Funds", "We are sorry, we are unable to complete this transation, as you do not have the required funds");
                }
                return;
            }

            player.BankAccounts.GiveMoney(-1 * Cost, true);
            airport.IsFlyingToLocation = true;
            Game.FadeScreenOut(1000, true);
            sender.Visible = false;
            airport.FlyToAirport(destinationAirport, FlightTime, HungerGain, ThirstGain, SleepGain);
        };

        rageMenu.AddItem(uIMenuItem);
    }
}

