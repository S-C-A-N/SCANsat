using System;
using System.Collections.Generic;
using System.Linq;
using SCANsat.SCAN_Platform;

namespace SCANsat.SCAN_UI.UI_Framework
{
	public class SCANlocalization : SCAN_ConfigNodeStorage
	{
		private SCANlanguagePack activePack;

		[Persistent]
		private List<SCANlanguagePack> languagePacks = new List<SCANlanguagePack>();

		public SCANlocalization(string path, string node)
		{
			FilePath = path;
			TopNodeName = node;

			if (!Load())
			{
				activePack = new SCANlanguagePack();
				languagePacks.Add(activePack);
				Save();
			}
		}

		public override void OnDecodeFromConfigNode()
		{
			activePack = languagePacks.FirstOrDefault(l => l.activePack);

			if (activePack == null)
				activePack = new SCANlanguagePack();
		}

		public SCANlanguagePack ActivePack
		{
			get { return activePack; }
		}
	}
}
