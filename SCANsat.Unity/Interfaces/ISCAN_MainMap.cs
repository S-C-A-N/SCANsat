using System;
using System.Collections.Generic;
using UnityEngine;

namespace SCANsat.Unity.Interfaces
{
	public interface ISCAN_MainMap
	{
		string Version { get; }

		bool IsVisible { get; set; }

		bool Color { get; set; }

		bool MapType { get; set; }

		bool Minimized { get; set; }

		Vector2 Position { get; set; }

		Dictionary<Guid, string> VesselInfoList { get; }

		void ClampToScreen(RectTransform rect);

		void OpenBigMap();

		void OpenInstruments();

		void OpenZoomMap();

		void OpenSettings();

		void OpenOverlay();

		string VesselInfo(Guid id);

		void Update();
	}
}
