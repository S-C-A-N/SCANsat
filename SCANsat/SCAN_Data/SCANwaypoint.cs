using System;
using FinePrint.Contracts.Parameters;
using FinePrint;
using FinePrint.Utilities;

namespace SCANsat.SCAN_Data
{
	public class SCANwaypoint
	{
		internal SCANwaypoint(SurveyWaypointParameter p)
		{
			way = reflectWaypoint(p);
			band = reflectFlightBand(p);
			name = way.name;
			longitude = SCANUtil.fixLonShift(way.longitude);
			latitude = SCANUtil.fixLatShift(way.latitude);
		}

		private Waypoint way;
		private string name;
		private double longitude;
		private double latitude;
		private FlightBand band;

		public Waypoint Way
		{
			get { return way; }
		}

		public string Name
		{
			get { return name; }
		}

		public double Longitude
		{
			get { return longitude; }
		}

		public double Latitude
		{
			get { return latitude; }
		}

		public FlightBand Band
		{
			get { return band; }
		}

		private Waypoint reflectWaypoint(SurveyWaypointParameter p)
		{
			if (SCANmainMenuLoader.FinePrintWaypoint)
				return SCANreflection.FinePrintWaypointObject(p);

			return null;
		}

		private FlightBand reflectFlightBand(SurveyWaypointParameter p)
		{
			if (SCANmainMenuLoader.FinePrintFlightBand)
				return SCANreflection.FinePrintFlightBandValue(p);

			return FlightBand.NONE;
		}
	}
}
