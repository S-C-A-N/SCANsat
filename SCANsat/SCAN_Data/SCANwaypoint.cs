#region license
/* 
 * [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 *
 * SCANwaypoint - An object to store information about FinePrint waypoints
 * 
 * Copyright (c)2014 David Grandy <david.grandy@gmail.com>;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
 */
#endregion

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
			if (way != null)
			{
				band = reflectFlightBand(p);
				root = p.Root;
				param = p;
				name = way.name;
				longitude = SCANUtil.fixLonShift(way.longitude);
				latitude = SCANUtil.fixLatShift(way.latitude);
				landingTarget = false;
			}
		}

		internal SCANwaypoint(StationaryPointParameter p)
		{
			way = reflectWaypoint(p);
			if (way != null)
			{
				band = FlightBand.NONE;
				root = p.Root;
				param = p;
				name = way.name;
				longitude = SCANUtil.fixLonShift(way.longitude);
				latitude = SCANUtil.fixLatShift(way.latitude);
				landingTarget = false;
			}
		}

		internal SCANwaypoint(Waypoint p)
		{
			way = p;
			band = FlightBand.NONE;
			root = p.contractReference;
			param = null;
			name = way.name;
			longitude = SCANUtil.fixLonShift(way.longitude);
			latitude = SCANUtil.fixLatShift(way.latitude);
			landingTarget = false;
		}

		public SCANwaypoint(double lat, double lon, string n)
		{
			way = null;
			band = FlightBand.NONE;
			root = null;
			param = null;
			name = n;
			longitude = SCANUtil.fixLonShift(lon);
			latitude = SCANUtil.fixLatShift(lat);
			landingTarget = true;
		}

		private Waypoint way;
		private string name;
		private double longitude;
		private double latitude;
		private FlightBand band;
		private Contract root;
		private ContractParameter param;
		private bool landingTarget;

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

		public bool LandingTarget
		{
			get { return landingTarget; }
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
