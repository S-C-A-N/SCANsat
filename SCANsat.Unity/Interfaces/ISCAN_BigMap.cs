using System;
using System.Collections.Generic;
using UnityEngine;

namespace SCANsat.Unity.Interfaces
{
	public interface ISCAN_BigMap
	{
		string Version { get; }

		string Readout { get; }

		string CurrentProjection { get; set; }

		string CurrentMapType { get; set; }

		string CurrentResource { get; set; }

		string CurrentCelestialBody { get; set; }

		bool IsVisible { get; set; }

		bool ColorToggle { get; set; }

		bool GridToggle { get; set; }

		bool OrbitToggle { get; set; }

		bool WaypointToggle { get; set; }

		bool AnomalyToggle { get; set; }

		bool FlagToggle { get; set; }

		bool AsteroidToggle { get; set; }

		bool LegendToggle { get; set; }

		bool ResourceToggle { get; set; }

		bool ShowOrbit { get; }

		bool ShowWaypoint { get; }

		float Scale { get; }

		Vector2 Position { get; set; }

		Vector2 Size { get; set; }

		IList<string> Projections { get; }

		IList<string> MapTypes { get; }

		IList<string> Resources { get; }

		IList<string> CelestialBodies { get; }

		void RefreshMap();

		void OpenMainMap();

		void OpenInstruments();

		void OpenZoomMap();

		void OpenSettings();

		void OpenOverlay();

		void ExportMap();

		void Update();

		void OnGUI();
	}
}
