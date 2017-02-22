#region license
/* 
 *  [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 *
 * SCANsat - Skins and Styles setup
 * 
 * Copyright (c)2013 damny;
 * Copyright (c)2014 David Grandy <david.grandy@gmail.com>;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
 *
 */
#endregion

using SCANsat.SCAN_Platform;
using SCANsat.SCAN_Platform.Palettes;
using palette = SCANsat.SCAN_UI.UI_Framework.SCANpalette;
using UnityEngine;


namespace SCANsat.SCAN_UI.UI_Framework
{
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	class SCANskins: SCAN_MBE
	{		
		internal static Texture2D SCAN_WaypointIcon;
		internal static Texture2D SCAN_MechJebIcon;
		internal static Texture2D SCAN_TargetIcon;

		//App Launcher Textures
		internal static Texture2D SCAN_SmallMapAppIcon;
		internal static Texture2D SCAN_BigMapAppIcon;

		protected override void OnGUI_FirstRun()
		{
			initializeTextures();
			initializeColors();
		}

		private static void initializeTextures()
		{
			SCAN_SmallMapAppIcon = GameDatabase.Instance.GetTexture("SCANsat/Icons/SCANsat_AppLauncherSmall_Icon", false);
			SCAN_BigMapAppIcon = GameDatabase.Instance.GetTexture("SCANsat/Icons/SCANsat_AppLauncherLarge_Icon", false);
			SCAN_WaypointIcon = GameDatabase.Instance.GetTexture("SCANsat/Icons/SCAN_WayPointIcon", false);
			SCAN_MechJebIcon = GameDatabase.Instance.GetTexture("SCANsat/Icons/SCAN_MechJebIcon", false);
			SCAN_TargetIcon = GameDatabase.Instance.GetTexture("SCANsat/Icons/SCAN_TargetIcon", false);
		}

		private static void initializeColors()
		{
			palette.CurrentPalettes = palette.setCurrentPalettesType(Palette.Kind.Diverging, 7);
		}
		
	}
}
