using System;
using System.Collections;
using System.IO;
using System.Threading;
using UnityEngine;
using SCANsat.SCAN_Data;
using SCANsat.SCAN_Platform;

namespace SCANsat.SCAN_Map
{
	public class SCANmapExporter : SCAN_MBE
	{
		private bool exporting;
		private volatile bool threadRunning, threadFinished;

		public bool Exporting
		{
			get { return exporting; }
		}

		public void exportPNG(SCANmap map, SCANdata data)
		{
			exporting = true;

			if (map == null)
				return;

			if (data == null)
				return;

			string path = Path.Combine(new DirectoryInfo(KSPUtil.ApplicationRootPath).FullName, "GameData/SCANsat/PluginData/").Replace("\\", "/");
			string mode = "";

			switch (map.MType)
			{
				case mapType.Altimetry: mode = "elevation"; break;
				case mapType.Slope: mode = "slope"; break;
				case mapType.Biome: mode = "biome"; break;
			}

			if (map.ResourceActive && SCANconfigLoader.GlobalResource && !string.IsNullOrEmpty(SCANcontroller.controller.resourceSelection))
				mode += "-" + SCANcontroller.controller.resourceSelection;
			if (SCANcontroller.controller.colours == 1)
				mode += "-grey";

			string baseFileName = string.Format("{0}_{1}_{2}x{3}", map.Body.name, mode, map.Map.width, map.Map.height);

			if (map.Projection != MapProjection.Rectangular)
				baseFileName += "_" + map.Projection.ToString();

			string filename = baseFileName;

			filename += ".png";

			string fullPath = Path.Combine(path, filename);

			File.WriteAllBytes(fullPath, map.Map.EncodeToPNG());

			ScreenMessages.PostScreenMessage("SCANsat Map saved: GameData/SCANsat/PluginData/" + filename, 8, ScreenMessageStyle.UPPER_CENTER);

			SCANUtil.SCANlog("Map of [{0}] saved\nMap Size: {1} X {2}\nMinimum Altitude: {3:F0}m; Maximum Altitude: {4:F0}m\nPixel Width At Equator: {5:F6}m", map.Body.theName, map.Map.width, map.Map.height, data.TerrainConfig.MinTerrain, data.TerrainConfig.MaxTerrain, (map.Body.Radius * 2 * Math.PI) / (map.Map.width * 1f));

			if (SCANcontroller.controller.exportCSV && map.MType == mapType.Altimetry)
				StartCoroutine(exportCSV(path, baseFileName, map, data));
			else
				exporting = false;
		}

		private IEnumerator exportCSV(string filePath, string fileName, SCANmap map, SCANdata data)
		{
			int timer = 0;

			SCANdata copy = new SCANdata(data);

			float[,] copyHeightMap = new float[map.MapWidth, map.MapHeight];

			Array.Copy(map.Big_HeightMap, copyHeightMap, map.MapWidth * map.MapHeight);

			Thread t = new Thread(() => exportThread(filePath, fileName, map, copy, copyHeightMap));
			threadFinished = false;
			threadRunning = true;
			t.Start();

			while (threadRunning && timer < 2000)
			{
				timer++;
				yield return null;
			}

			SCANUtil.SCANlog(".csv data file export complete; exported over {0} frames\nFile saved to GameData/SCANsat/PluginData/{1}_data.csv", timer, fileName);

			copy = null;
			copyHeightMap = null;
			exporting = false;

			if (timer >= 2000)
			{
				Debug.LogError("[SCANsat] Something went wrong while exporting .csv data file/nCanceling export thread...");
				t.Abort();
				threadRunning = false;
				yield break;
			}

			if (!threadFinished)
			{
				Debug.LogError("[SCANsat] Something went wrong while exporting .csv data file/nExport thread has been interrupted...");
				yield break;
			}
		}

		private void exportThread(string path, string fileName, SCANmap map, SCANdata copyData, float[,] copyMap)
		{
			try
			{
				using (StreamWriter w = new StreamWriter(Path.Combine(path, fileName + "_data" + ".csv")))
				{
					string line = "Row,Column,Lat,Long,Height";
					w.WriteLine(line);
					for (int i = 0; i < map.Map.height; i++)
					{
						for (int j = 0; j < map.Map.width; j++)
						{
							double lat = (i * 1.0f / map.MapScale) - 90f;
							double lon = (j * 1.0f / map.MapScale) - 180f;
							double la = lat, lo = lon;
							lat = map.unprojectLatitude(lo, la);
							lon = map.unprojectLongitude(lo, la);

							if (double.IsNaN(lat) || double.IsNaN(lon) || lat < -90 || lat > 90 || lon < -180 || lon > 180)
								continue;

							if (!SCANUtil.isCovered(lon, lat, copyData, SCANtype.Altimetry))
								continue;

							float terrain = map.terrainElevation(lon, lat, copyMap, copyData);

							line = string.Format("{0},{1},{2:F3},{3:F3},{4:F3}", i, j, lat, lon, terrain);

							w.WriteLine(line);
						}
						w.Flush();
					}
				}
				threadFinished = true;
			}
			catch
			{
				threadFinished = false;
			}
			finally
			{
				threadRunning = false;
			}
		}
	}
}
