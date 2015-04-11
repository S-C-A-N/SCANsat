using System;
using Contracts;
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
			root = p.Root;
			param = p;
			name = way.name;
			longitude = SCANUtil.fixLonShift(way.longitude);
			latitude = SCANUtil.fixLatShift(way.latitude);
			logWaypointData();
		}

		internal SCANwaypoint(StationaryPointParameter p)
		{
			way = reflectWaypoint(p);
			band = FlightBand.NONE;
			root = p.Root;
			param = p;
			name = way.name;
			longitude = SCANUtil.fixLonShift(way.longitude);
			latitude = SCANUtil.fixLatShift(way.latitude);
			logWaypointData();
		}

		internal SCANwaypoint(Waypoint p)
		{
			way = p;
			band = FlightBand.NONE;
			root = p.contractReference;
			p = null;
			name = way.name;
			longitude = SCANUtil.fixLonShift(way.longitude);
			latitude = SCANUtil.fixLatShift(way.latitude);
			logWaypointData();
		}

		private void logWaypointData()
		{
			SCANUtil.SCANdebugLog("Waypoint Assigned -------------");
			SCANUtil.SCANdebugLog("Waypoint Name:------------{0}", name);
			SCANUtil.SCANdebugLog("Waypoint Flight Band:------{0}", band);
			SCANUtil.SCANdebugLog("Waypoint Latitude:----------{0}", latitude);
			SCANUtil.SCANdebugLog("Waypoint Longitude:------{0}", longitude);
		}

		private Waypoint way;
		private string name;
		private double longitude;
		private double latitude;
		private FlightBand band;
		private Contract root;
		private ContractParameter param;

		public Waypoint Way
		{
			get { return way; }
		}

		public string Name
		{
			get { return name; }
		}

		public Contract Root
		{
			get { return root; }
		}

		public ContractParameter Param
		{
			get { return param; }
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

		private Waypoint reflectWaypoint(StationaryPointParameter p)
		{
			if (SCANmainMenuLoader.FinePrintStationaryWaypoint)
				return SCANreflection.FinePrintStationaryWaypointObject(p);

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
