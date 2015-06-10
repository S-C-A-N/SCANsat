#region license
/* 
 * [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 *
 * SCANterrainConfig - Serializable object for storing data about each planet's terrain options and color palette
 * 
 * Copyright (c)2014 David Grandy <david.grandy@gmail.com>;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
 */
#endregion

using System.Linq;
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
		private string clampHeight;
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
		private float? clampTerrain;

		private float defaultMinHeight, defaultMaxHeight;
		private Palette defaultPalette;
		private int defaultPaletteSize;
		private bool defaultReverse, defaultDiscrete;
		private float? defaultClamp;

		internal SCANterrainConfig(float min, float max, float? clamp, Palette color, int size, bool reverse, bool discrete, CelestialBody b)
		{
			minHeightRange = min;
			maxHeightRange = max;
			clampTerrain = clamp;
			if (clampTerrain == null)
				clampHeight = "Null";
			else
				clampHeight = clampTerrain.Value.ToString("F0");
			colorPal = color;
			paletteName = colorPal.name;
			paletteSize = size;
			paletteReverse = reverse;
			paletteDiscrete = discrete;
			body = b;
			name = body.name;
			index = body.flightGlobalsIndex;

			setDefaultValues();
		}

		public SCANterrainConfig()
		{
		}

		internal SCANterrainConfig(SCANterrainConfig copy)
		{
			minHeightRange = copy.minHeightRange;
			maxHeightRange = copy.maxHeightRange;
			clampTerrain = copy.clampTerrain;
			clampHeight = copy.clampHeight;
			colorPal = copy.colorPal;
			paletteName = copy.paletteName;
			paletteSize = copy.paletteSize;
			paletteReverse = copy.paletteReverse;
			paletteDiscrete = copy.paletteDiscrete;
			body = copy.body;
			name = copy.name;
		}

		public override void OnDecodeFromConfigNode()
		{
			body = FlightGlobals.Bodies.FirstOrDefault(b => b.flightGlobalsIndex == index);
			if (body != null)
				name = body.name;

			colorPal = SCANUtil.paletteLoader(paletteName, paletteSize);

			float tempClamp = 0;
			if (clampHeight == "Null" || clampHeight == "null" || string.IsNullOrEmpty(clampHeight))
				clampTerrain = null;
			else if (float.TryParse(clampHeight, out tempClamp))
				clampTerrain = tempClamp;
			else
				clampTerrain = null;

			setDefaultValues();

			SCANUtil.SCANdebugLog("SCANsat Terrain Config Decode");
			SCANUtil.SCANdebugLog("-------->Body Name             =>   {0}", name);
			SCANUtil.SCANdebugLog("-------->Body Index            =>   {0}", index);
			SCANUtil.SCANdebugLog("-------->Min Height Range      =>   {0}", minHeightRange);
			SCANUtil.SCANdebugLog("-------->Max Height Range      =>   {0}", maxHeightRange);
			SCANUtil.SCANdebugLog("-------->Clamp Height          =>   {0}", clampHeight);
			SCANUtil.SCANdebugLog("-------->Palette Name          =>   {0}", paletteName);
			SCANUtil.SCANdebugLog("-------->Palette Size          =>   {0}", paletteSize);
			SCANUtil.SCANdebugLog("-------->Palette Reverse       =>   {0}", paletteReverse);
			SCANUtil.SCANdebugLog("-------->Palette Discrete      =>   {0}", paletteDiscrete);
		}

		private void setDefaultValues()
		{
			defaultMinHeight = minHeightRange;
			defaultMaxHeight = maxHeightRange;
			defaultClamp = clampTerrain;
			defaultPalette = colorPal;
			defaultPaletteSize = paletteSize;
			defaultDiscrete = paletteDiscrete;
			defaultReverse = paletteReverse;
		}

		public override void OnEncodeToConfigNode()
		{
			SCANUtil.SCANdebugLog("Saving Terrain Node");
			if (clampTerrain == null)
				clampHeight = "Null";
			else
				clampHeight = clampTerrain.Value.ToString("F0");

			paletteName = colorPal.name;
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

		public float DefaultMinHeight
		{
			get { return defaultMinHeight; }
		}

		public float DefaultMaxHeight
		{
			get { return defaultMaxHeight; }
		}

		public float? DefaultClampHeight
		{
			get { return defaultClamp; }
		}

		public Palette DefaultPalette
		{
			get { return defaultPalette; }
		}

		public int DefaultPaletteSize
		{
			get { return defaultPaletteSize; }
		}

		public bool DefaultReverse
		{
			get { return defaultReverse; }
		}

		public bool DefaultDiscrete
		{
			get { return defaultDiscrete; }
		}
	}

}
