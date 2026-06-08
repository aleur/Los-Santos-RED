using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class FlightOptions
{
    public FlightOptions()
    {
    }

    public FlightOptions(string iD, List<AirportFlight> airportFlights)
    {
        ID = iD;
        AirportFlights = airportFlights;
    }

    public string ID { get; set; }
    public List<AirportFlight> AirportFlights { get; set; } = new List<AirportFlight>();
}

