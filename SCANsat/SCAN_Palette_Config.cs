using System;
using System.Collections.Generic;
using System.Linq;
using SCANsat.SCAN_Palettes;
using SCANsat.SCAN_Platform;
using UnityEngine;
using palette = SCANsat.SCAN_UI.UI_Framework.SCANcolorUtil;

namespace SCANsat
{
	public class SCAN_Palette_Config : SCAN_ConfigNodeStorage
	{
		[Persistent]
		private List<SCANPaletteType> PaletteTypes = new List<SCANPaletteType>();

		private DictionaryValueList<SCANPaletteKind, SCANPaletteType> MasterPaletteTypeList = new DictionaryValueList<SCANPaletteKind, SCANPaletteType>();

		private SCANPalette _defaultPalette;

		public SCANPalette DefaultPalette
		{
			get
			{
				if (_defaultPalette == null)
					_defaultPalette = GenerateDefaultPalette();

				return _defaultPalette;
			}
		}

		public SCAN_Palette_Config(string filepath, string node)
		{
			FilePath = filepath;
			TopNodeName = filepath + "/" + node;

			if (!Load())
			{
				SaveDefaultPalettes();
				Save();
				LoadSavedCopy();
			}
			else
				SCANUtil.SCANlog("Palette File Loaded");

			_defaultPalette = GenerateDefaultPalette();
		}

		private SCANPalette GenerateDefaultPalette()
		{
			Color32[] c = new Color32[7] { (Color32)palette.xkcd_DarkPurple, (Color32)palette.xkcd_Cerulean, (Color32)palette.xkcd_ArmyGreen, (Color32)palette.xkcd_Yellow, (Color32)palette.xkcd_Red, (Color32)palette.xkcd_Magenta, (Color32)palette.xkcd_White };

			return new SCANPalette(c, "Default", SCANPaletteKind.Fixed, c.Length);
		}

		private void SaveDefaultPalettes()
		{
			SCANUtil.SCANlog("Generating default palettes");

			PaletteTypes = new List<SCANPaletteType>();

			PaletteTypes.Add(new SCANPaletteType("Diverging"));
			PaletteTypes.Add(new SCANPaletteType("Qualitative"));
			PaletteTypes.Add(new SCANPaletteType("Sequential"));
			PaletteTypes.Add(new SCANPaletteType("Fixed"));

			for (int i = 0; i < PaletteTypes.Count; i++)
				PaletteTypes[i].SaveDefaultPalettes();

			OnDecodeFromConfigNode();
		}

		public SCANPaletteType GetPaletteType(SCANPaletteKind kind)
		{
			if (MasterPaletteTypeList.Contains(kind))
				return MasterPaletteTypeList[kind];

			if (MasterPaletteTypeList.Count > 0)
				return MasterPaletteTypeList.At(0);

			return null;
		}

		public SCANPaletteGroup GetPaletteGroup(SCANPaletteKind kind, string name)
		{
			for (int i = 0; i < MasterPaletteTypeList.Count; i++)
			{
				SCANPaletteType type = MasterPaletteTypeList.At(i);

				if (type.Kind != kind)
					continue;

				SCANPaletteGroup group = type.GetPaletteGroup(name);

				return group;
			}

			return null;
		}

		public SCANPaletteGroup GetPaletteGroup(string name)
		{
			for (int i = 0; i < MasterPaletteTypeList.Count; i++)
			{
				SCANPaletteType type = MasterPaletteTypeList.At(i);

				SCANPaletteGroup group = type.GetPaletteGroup(name);

				if (group != null)
					return group;
			}

			return null;
		}

		public List<SCANPalette> GetPaletteList(SCANPaletteKind kind, int length)
		{
			for (int i = 0; i < MasterPaletteTypeList.Count; i++)
			{
				SCANPaletteType type = MasterPaletteTypeList.At(i);

				if (type.Kind != kind)
					continue;

				return type.GetPaletteList(length);
			}

			return null;
		}

		public override void OnDecodeFromConfigNode()
		{
			try
			{
				for (int i = PaletteTypes.Count - 1; i >= 0; i--)
				{
					SCANPaletteType p = PaletteTypes[i];

					if (p == null)
						continue;

					if (!MasterPaletteTypeList.Contains(p.Kind))
						MasterPaletteTypeList.Add(p.Kind, p);
				}
			}
			catch (Exception e)
			{
				SCANUtil.SCANlog("Error while loading SCANsat palettes config settings: {0}", e);
			}
		}

		public override void OnEncodeToConfigNode()
		{
			try
			{
				PaletteTypes = MasterPaletteTypeList.Values.ToList();
			}
			catch (Exception e)
			{
				SCANUtil.SCANlog("Error while saving SCANsat palettes config data: {0}", e);
			}
		}

	}
}
