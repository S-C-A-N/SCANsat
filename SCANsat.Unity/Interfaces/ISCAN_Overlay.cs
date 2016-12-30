using System;
using System.Collections.Generic;
using UnityEngine;

namespace SCANsat.Unity.Interfaces
{
	public interface ISCAN_Overlay
	{
		string Version { get; }

		string CurrentResource { get; }

		bool IsVisible { get; set; }

		bool Tooltips { get; set; }

		bool DrawOverlay { get; set; }

		bool DrawBiome { get; set; }

		bool DrawTerrain { get; set; }

		float Scale { get; }

		IList<string> Resources { get; }

		Vector2 Position { get; set; }

		void ClampToScreen(RectTransform rect);

		void DrawResource(string resource, bool isOn);

		void Refresh();

		void OpenSettings();

		void Update();
	}
}
