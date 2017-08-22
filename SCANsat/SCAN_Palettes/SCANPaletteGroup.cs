
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SCANsat.SCAN_Platform;

namespace SCANsat.SCAN_Palettes
{
	public class SCANPaletteGroup : SCAN_ConfigNodeStorage
	{
		[Persistent]
		private string PaletteName;
		[Persistent]
		private List<SCANPalette> Palettes = new List<SCANPalette>();

		private DictionaryValueList<int, SCANPalette> MasterPaletteList = new DictionaryValueList<int, SCANPalette>();

		private SCANPaletteKind _kind;

		public SCANPaletteGroup() { }

		public SCANPaletteGroup(string name, SCANPaletteKind kind)
		{
			PaletteName = name;
			_kind = kind;
		}

		public SCANPaletteKind Kind
		{
			get { return _kind; }
			set { _kind = value; }
		}

		public string _PaletteName
		{
			get { return PaletteName; }
		}

		public SCANPalette GetPalette(int length)
		{
			if (_kind == SCANPaletteKind.Fixed && MasterPaletteList.Count > 0)
				return MasterPaletteList.At(0);

			if (MasterPaletteList.Contains(length))
				return MasterPaletteList[length];

			if (MasterPaletteList.Count > 0)
				return MasterPaletteList.At(MasterPaletteList.Count - 1);

			return null;
		}

		public void SaveDefaultPalettes()
		{
			Palettes = new List<SCANPalette>();

			int count = 1;

			try
			{
				switch (_kind)
				{
					case SCANPaletteKind.Fixed:
						var fixedPalette = typeof(FixedColorPalettes);
						var fixedPaletteMethod = fixedPalette.GetMethod(PaletteName);
						var fixedColorPalette = fixedPaletteMethod.Invoke(null, null);
						Palettes.Add((SCANPalette)fixedColorPalette);
						OnDecodeFromConfigNode();
						return;
					case SCANPaletteKind.Diverging:
						count = 11;
						break;
					case SCANPaletteKind.Qualitative:
						count = 12;
						break;
					case SCANPaletteKind.Sequential:
						count = 9;
						break;
					default:
						break;
				}

				var brewerPalette = typeof(BrewerPalettes);
				var brewerPaletteMethod = brewerPalette.GetMethod(PaletteName);

				for (int i = 3; i <= count; i++)
				{
					var brewerColorPalette = brewerPaletteMethod.Invoke(null, new object[] { i });
					Palettes.Add((SCANPalette)brewerColorPalette);
				}
			}
			catch (Exception e)
			{
				SCANUtil.SCANlog("Error Generating Color Palettes: {0}", e);
			}

			OnDecodeFromConfigNode();
		}

		public override void OnDecodeFromConfigNode()
		{
			try
			{
				for (int i = Palettes.Count - 1; i >= 0; i--)
				{
					SCANPalette p = Palettes[i];

					if (p == null)
						continue;

					if (!MasterPaletteList.Contains(p.Count))
						MasterPaletteList.Add(p.Count, p);

					p.Name = PaletteName;
					p.Kind = _kind;
				}
			}
			catch (Exception e)
			{
				SCANUtil.SCANlog("Error while loading SCANsat palette config settings: {0}", e);
			}
		}

		public override void OnEncodeToConfigNode()
		{
			try
			{
				Palettes = MasterPaletteList.Values.ToList();
			}
			catch (Exception e)
			{
				SCANUtil.SCANlog("Error while saving SCANsat palette config data: {0}", e);
			}
		}
	}
}
