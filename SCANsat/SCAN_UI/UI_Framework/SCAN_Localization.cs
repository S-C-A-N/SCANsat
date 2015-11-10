using System;
using System.Collections.Generic;
using System.Linq;
using SCANsat.SCAN_Platform;

namespace SCANsat.SCAN_UI.UI_Framework
{
	public class SCAN_Localization : SCAN_ConfigNodeStorage
	{
		private SCANlanguagePack activePack;

		[Persistent]
		private List<SCANlanguagePack> Language_Packs = new List<SCANlanguagePack>();

		public SCAN_Localization(string path, string node)
		{
			FilePath = path;
			TopNodeName = path + "/" + node;

			if (!Load())
			{
				activePack = new SCANlanguagePack();
				Language_Packs.Add(activePack);
				Save();
				LoadSavedCopy();
			}
			else
			{
				SCANUtil.SCANlog("Language File Loaded...");
			}
		}

		public override void OnDecodeFromConfigNode()
		{
			activePack = Language_Packs.FirstOrDefault(l => l.activePack);

			if (activePack == null)
				activePack = new SCANlanguagePack();
		}

		public SCANlanguagePack ActivePack
		{
			get { return activePack; }
		}
	}
}
