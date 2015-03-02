using System;
using SCANsat.SCAN_Platform.Palettes;

namespace SCANsat.SCAN_Data
{
	public class SCANterrainConfig
	{
		private float minTerrain;
		private float maxTerrain;
		private float? clampTerrain;
		private Palette colorPal;
		private int palSize;
		private bool palRev;
		private bool palDis;
		private CelestialBody body;

		internal SCANterrainConfig(float min, float max, float? clamp, Palette color, int size, bool reverse, bool discrete, CelestialBody b)
		{
			minTerrain = min;
			maxTerrain = max;
			clampTerrain = clamp;
			colorPal = color;
			palSize = size;
			palRev = reverse;
			palDis = discrete;
			body = b;
		}

		public float MinTerrain
		{
			get { return minTerrain; }
		}

		public float MaxTerrain
		{
			get { return maxTerrain; }
		}

		public float? ClampTerrain
		{
			get { return clampTerrain; }
		}

		public Palette ColorPal
		{
			get { return colorPal; }
		}

		public int PalSize
		{
			get { return palSize; }
		}

		public bool PalRev
		{
			get { return palRev; }
		}

		public bool PalDis
		{
			get { return palDis; }
		}

		public CelestialBody Body
		{
			get { return body; }
		}
	}

}
