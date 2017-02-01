using System;
using UnityEngine;

namespace SCANsat.Unity.Interfaces
{
	public interface ISCAN_Instruments
	{
		string Version { get; }

		string Readout { get; }

		string ResourceName { get; }

		string TypingText { get; }

		string AnomalyText { get; }

		bool IsVisible { get; set; }

		bool ResourceButtons { get; }

		bool Anomaly { get; }

		bool TooltipsOn { get; }

		float Scale { get; }

		Canvas TooltipCanvas { get; }

		Texture AnomalyCamera { get; }

		Vector2 Position { get; set; }

		void ClampToScreen(RectTransform rect);

		void NextResource();

		void PreviousResource();

		void Update();
	}
}
