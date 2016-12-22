using System;
using UnityEngine;

namespace SCANsat.Unity.Interfaces
{
	public interface ISCAN_Settings
	{
		string Version { get; }

		string SensorCount { get; }

		int TimeWarp { get; set; }

		int MapWidth { get; set; }

		int Interpolation { get; set; }

		int MapHeight { get; set; }

		int BiomeMapHeight { get; set; }

		float Transparency { get; set; }

		float StockThresholdValue { get; set; }

		float UIScale { get; set; }

		bool IsVisible { get; set; }

		bool BackgroundScanning { get; set; }

		bool GroundTracks { get; set; }

		bool ActiveGround { get; set; }

		bool OverlayTooltips { get; set; }

		bool WindowTooltips { get; set; }

		bool StockToolbar { get; set; }

		bool ToolbarMenu { get; set; }

		bool StockUIStyle { get; set; }

		bool BiomeLock { get; set; }

		bool NarrowBand { get; set; }

		bool InstantScan { get; set; }

		bool DisableStock { get; set; }

		bool StockThreshold { get; set; }

		bool GreyScale { get; set; }

		bool ExportCSV { get; set; }

		bool ShowSCANsatReset { get; }

		bool ShowStockReset { get; }

		bool ShowMapFill { get; }

		void ClampToScreen(RectTransform rect);

		void ResetCurrent();

		void ResetAll();

		void ResetSCANResource();

		void ResetStockResource();

		void FillCurrent();

		void FillAll();

		void OpenColor();

		void ResetWindows();

		void Update();

		void ToggleBody(string name);

		string BodyPercentage(string bodyName);
	}
}
