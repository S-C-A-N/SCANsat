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

		bool TooltipsOn { get; }

		bool MapGenerating { get; }

		float Scale { get; }

		Canvas TooltipCanvas { get; }

		Vector2 Position { get; set; }

		Dictionary<Guid, MapLabelInfo> VesselInfoList { get; }

		void ClampToScreen(RectTransform rect);

		void OpenBigMap();

		void OpenInstruments();

		void OpenZoomMap();

		void OpenSettings();

		void OpenOverlay();

		void ChangeToVessel(Guid id);

		string VesselInfo(Guid id);

		Sprite VesselType(Guid id);

		Vector2 VesselPosition(Guid id);

		void Update();
	}
}
