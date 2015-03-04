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
		private ConfigNode node;

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

		internal SCANterrainConfig(SCANterrainConfig copy)
		{
			minTerrain = copy.minTerrain;
			maxTerrain = copy.maxTerrain;
			clampTerrain = copy.clampTerrain;
			colorPal = copy.colorPal;
			palSize = copy.palSize;
			palRev = copy.palRev;
			palDis = copy.palDis;
			body = copy.body;
		}

		internal void setNode(ConfigNode n)
		{
			node = n;
		}

		internal void updateNode()
		{
			node.SetValue("minHeightRange", minTerrain.ToString("F0"));
			node.SetValue("maxHeightRange", maxTerrain.ToString("F0"));
			if (clampTerrain != null)
			{
				if (node.HasValue("clampHeight"))
					node.SetValue("clampHeight", clampTerrain.Value.ToString("F0"));
				else
					node.AddValue("clampHeight", clampTerrain.Value.ToString("F0"));
			}
			else
			{
				if (node.HasValue("clampHeight"))
					node.RemoveValue("clampHeight");
			}
			node.SetValue("paletteName", colorPal.name);
			node.SetValue("paletteSize", palSize.ToString());
			node.SetValue("paletteReverse", palRev.ToString());
			node.SetValue("palatteDiscrete", palDis.ToString());
		}

		public float MinTerrain
		{
			get { return minTerrain; }
			internal set
			{
				if (value < maxTerrain)
					minTerrain = value;
			}
		}

		public float MaxTerrain
		{
			get { return maxTerrain; }
			internal set
			{
				if (value > minTerrain)
					maxTerrain = value;
			}
		}

		public float? ClampTerrain
		{
			get { return clampTerrain; }
			internal set
			{
				if (value == null)
					clampTerrain = null;
				else if (value > minTerrain && value < maxTerrain)
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
			get { return palSize; }
			internal set { palSize = value; }
		}

		public bool PalRev
		{
			get { return palRev; }
			internal set { palRev = value; }
		}

		public bool PalDis
		{
			get { return palDis; }
			internal set { palDis = value; }
		}

		public CelestialBody Body
		{
			get { return body; }
		}
	}

}
