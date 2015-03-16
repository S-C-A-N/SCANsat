using System;
using SCANsat.SCAN_Platform;
using SCANsat.SCAN_Platform.Palettes;

namespace SCANsat.SCAN_Data
{
	public class SCANterrainConfig : SCAN_ConfigNodeStorage
	{
		[Persistent]
		private string name;
		[Persistent]
		private int index;
		[Persistent]
		private float minHeightRange;
		[Persistent]
		private float maxHeightRange;
		[Persistent]
		private float? clampTerrain;
		[Persistent]
		private string paletteName;
		[Persistent]
		private int paletteSize;
		[Persistent]
		private bool paletteReverse;
		[Persistent]
		private bool paletteDiscrete;

		private Palette colorPal;
		private CelestialBody body;

		internal SCANterrainConfig(float min, float max, float? clamp, Palette color, int size, bool reverse, bool discrete, CelestialBody b)
		{
			minHeightRange = min;
			maxHeightRange = max;
			clampTerrain = clamp;
			colorPal = color;
			paletteSize = size;
			paletteReverse = reverse;
			paletteDiscrete = discrete;
			body = b;
			index = body.flightGlobalsIndex;
		}

		internal SCANterrainConfig(SCANterrainConfig copy)
		{
			minHeightRange = copy.minHeightRange;
			maxHeightRange = copy.maxHeightRange;
			clampTerrain = copy.clampTerrain;
			colorPal = copy.colorPal;
			paletteSize = copy.paletteSize;
			paletteReverse = copy.paletteReverse;
			paletteDiscrete = copy.paletteDiscrete;
			body = copy.body;
		}

		public float MinTerrain
		{
			get { return minHeightRange; }
			internal set
			{
				if (value < maxHeightRange)
					minHeightRange = value;
			}
		}

		public float MaxTerrain
		{
			get { return maxHeightRange; }
			internal set
			{
				if (value > minHeightRange)
					maxHeightRange = value;
			}
		}

		public float? ClampTerrain
		{
			get { return clampTerrain; }
			internal set
			{
				if (value == null)
					clampTerrain = null;
				else if (value > minHeightRange && value < maxHeightRange)
					clampTerrain = value;
			}
		}

		public Palette ColorPal
		{
			get { return colorPal; }
			internal set { colorPal = value; }
		}

		public int PalSize
		{
			get { return paletteSize; }
			internal set { paletteSize = value; }
		}

		public bool PalRev
		{
			get { return paletteReverse; }
			internal set { paletteReverse = value; }
		}

		public bool PalDis
		{
			get { return paletteDiscrete; }
			internal set { paletteDiscrete = value; }
		}

		public CelestialBody Body
		{
			get { return body; }
		}

		public string Name
		{
			get { return name; }
		}
	}

}
