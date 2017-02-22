#region license
/*  [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 *
 * ISCAN_Settings - Interface for transfer of settings information
 * 
 * Copyright (c)2013 damny;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 DMagic
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
 */
#endregion

using System;
using System.Collections.Generic;
using UnityEngine;

namespace SCANsat.Unity.Interfaces
{
	public interface ISCAN_Settings
	{
		string Version { get; }

		string SensorCount { get; }

		string DataResetCurrent { get; }

		string DataResetAll { get; }

		string SCANResourceResetCurrent { get; }

		string SCANResourceResetAll { get; }

		string StockResourceResetCurrent { get; }

		string StockResourceResetAll { get; }

		string ModuleManagerWarning { get; }

		string SaveToConfig { get; }

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

		bool MapGenSpeed { get; set; }

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

		bool LockInput { get; set; }

		bool ModuleManager { get; }

		Canvas TooltipCanvas { get; }

		Vector2 Position { set; }

		IList<string> BackgroundBodies { get; }

		ISCAN_Color ColorInterface { get; }

		void ClampToScreen(RectTransform rect);

		void ResetCurrent();

		void ResetAll();

		void ResetSCANResourceCurrent();

		void ResetSCANResourceAll();

		void ResetStockResourceCurrent();

		void ResetStockResourceAll();

		void FillCurrent();

		void FillAll();

		void ResetWindows();

		void Update();

		void ToggleBody(string name);

		double BodyPercentage(string bodyName);
	}
}
