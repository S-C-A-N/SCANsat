/*
 *  [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 *
 * SCANtoolbar -	optional integration with Blizzy's toolvar
 *
 * Copyright (c)2014 David Grandy <david.grandy@gmail.com>;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
 *
 * Created by David to allow the SCANsat plugin to function through the toolbar interface
 */

using System.IO;
using UnityEngine;

namespace SCANsat
{
	[KSPAddon(KSPAddon.Startup.Flight, false)]
    class SCANtoolbar : MonoBehaviour
    {
		private IButton SCANButton;
		private IButton MapButton;
		private IButton SmallButton;

		internal SCANtoolbar ()
        {
			if (!ToolbarManager.ToolbarAvailable) return; // bail if we don't have a toolbar

			SCANButton	= ToolbarManager.Instance.add ("SCANsat" , "UIMenu");
			MapButton 	= ToolbarManager.Instance.add ("SCANsat" , "BigMap");
			SmallButton 	= ToolbarManager.Instance.add ("SCANsat" , "SmallMap");

            //Fall back to some default toolbar icons if someone deletes the SCANsat icons or puts them in the wrong folder
            if (File.Exists(Path.Combine(new DirectoryInfo(KSPUtil.ApplicationRootPath).FullName, "GameData/SCANsat/Icons/SCANsat_Icon.png").Replace("\\", "/")))
                SCANButton.TexturePath 	= "SCANsat/Icons/SCANsat_Icon"; // S.C.A.N
            else 
                SCANButton.TexturePath = "000_Toolbar/toolbar-dropdown";
            if (File.Exists(Path.Combine(new DirectoryInfo(KSPUtil.ApplicationRootPath).FullName, "GameData/SCANsat/Icons/SCANsat_Map_Icon.png").Replace("\\", "/")))
			    MapButton.TexturePath 	= "SCANsat/Icons/SCANsat_Map_Icon"; // from in-game biome map of Kerbin
            else
                MapButton.TexturePath = "000_Toolbar/move-cursor";
            if (File.Exists(Path.Combine(new DirectoryInfo(KSPUtil.ApplicationRootPath).FullName, "GameData/SCANsat/Icons/SCANsat_SmallMap_Icon.png").Replace("\\", "/")))
			    SmallButton.TexturePath 	= "SCANsat/Icons/SCANsat_SmallMap_Icon"; // from unity, edited by DG
            else
                SmallButton.TexturePath = "000_Toolbar/resize-cursor";
                
			SCANButton.ToolTip 	= "SCANsat";
			MapButton.ToolTip 	= "SCANsat Big Map";
			SmallButton.ToolTip = "SCANsat Small Map";

			SCANButton.OnClick 	+= (e) => toggleMenu (SCANButton);
			MapButton.OnClick 	+= (e) => SCANui.bigmap_visible = !SCANui.bigmap_visible;
			SmallButton.OnClick += (e) => SCANui.minimode = (SCANui.minimode == 0 ? 2 : -SCANui.minimode);
        }

		private void toggleMenu ( IButton menu )
        {
			if (!ToolbarManager.ToolbarAvailable) return; // bail if we don't have a toolbar
			if (menu.Drawable == null)
				createMenu (menu);
			else
				destroyMenu (menu);
        }

		private void createMenu ( IButton menu )
        {
			if (!ToolbarManager.ToolbarAvailable) return; // bail if we don't have a toolbar

			PopupMenuDrawable list = new PopupMenuDrawable ();

			IButton smallMap = list.AddOption("Small Map");
			IButton instrument = list.AddOption("Instruments");
			IButton bigMap = list.AddOption ("Big Map");
			IButton settings = list.AddOption ("Settings");

			smallMap.OnClick += (e2) => SCANui.minimode = (SCANui.minimode == 0 ? 2 : -SCANui.minimode);
			instrument.OnClick += (e2) => SCANui.instruments_visible = !SCANui.instruments_visible;
			bigMap.OnClick += (e2) => SCANui.bigmap_visible = !SCANui.bigmap_visible;
			settings.OnClick += (e2) => {
				if (!SCANui.settings_visible)
					SCANcontroller.controller.Resources(FlightGlobals.currentMainBody);
				SCANui.settings_visible = !SCANui.settings_visible;
			};
			list.OnAnyOptionClicked += (  ) => destroyMenu (menu);
			menu.Drawable = list;
		}

		private void destroyMenu ( IButton menu )
        {
			if (!ToolbarManager.ToolbarAvailable) return; // bail if we don't have a toolbar
			((PopupMenuDrawable)menu.Drawable).Destroy ();
			menu.Drawable = null;
        }

		internal void OnDestroy ()
        {
			if (!ToolbarManager.ToolbarAvailable) return; // bail if we don't have a toolbar
			SCANButton.Destroy ();
			MapButton.Destroy ();
			SmallButton.Destroy ();
        }

    }       
}
