using System;
using System.Collections.Generic;
using UnityEngine;

namespace SCANsat.Unity.Interfaces
{
	public interface ISCAN_BigMap
	{
		string Version { get; }

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

		bool ShowResource { get; }

		int OrbitSteps { get; }

		float Scale { get; }

		Canvas MainCanvas { get; }

		Vector2 Position { get; set; }

		Vector2 Size { get; set; }

		Vector2 MapScreenPosition { get; set; }

		Texture2D LegendImage { get; }

		IList<string> Projections { get; }

		IList<string> MapTypes { get; }

		IList<string> Resources { get; }

		IList<string> CelestialBodies { get; }

		IList<string> LegendLabels { get; }

		Dictionary<Guid, MapLabelInfo> FlagInfoList { get; }

		Dictionary<string, MapLabelInfo> AnomalyInfoList { get; }

		Dictionary<int, MapLabelInfo> WaypointInfoList { get; }

		KeyValuePair<Guid, MapLabelInfo> VesselInfo { get; }

		string MapInfo(Vector2 rectPosition);

		void RefreshMap();

		void OpenMainMap();

		void OpenInstruments();

		void OpenZoomMap();

		void OpenSettings();

		void OpenOverlay();

		void ExportMap();

		void Update();

		void OnGUI();

		SimpleLabelInfo OrbitInfo(int index);

		Vector2 VesselPosition();
	}
}
