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

using UnityEngine;
using Toolbar;

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
			SCANButton = ToolbarManager.Instance.add ("SCANsat" , "UIMenu");           
			SCANButton.TexturePath = "SCANsat/SCANsat_Icon";
			SCANButton.ToolTip = "SCANsat";
			SCANButton.OnClick += (e) => toggleMenu (SCANButton);

			MapButton = ToolbarManager.Instance.add ("SCANsat" , "BigMap");
			//Map texture from SCANsat map created from in-game biome map of Kerbin
			MapButton.TexturePath = "SCANsat/SCANsat_Map_Icon";
			MapButton.ToolTip = "SCANsat Big Map";
			MapButton.OnClick += (e) => SCANui.bigmap_visible = !SCANui.bigmap_visible;

			SmallButton = ToolbarManager.Instance.add ("SCANsat" , "SmallMap");
			//The toolbar icon texture was pulled out of the unity package and edited by me to match the old SCANsat color scheme
			SmallButton.TexturePath = "SCANsat/SCANsat_SmallMap_Icon";
			SmallButton.ToolTip = "SCANsat Small Map";
			SmallButton.OnClick += (e) => SCANui.minimode = (SCANui.minimode == 0 ? 2 : -SCANui.minimode);
        }

		private void LateUpdate ()
        {
			buttonVisible (FlightGlobals.ActiveVessel.FindPartModulesImplementing<SCANsat> ().Count > 0);
        }

		private void buttonVisible ( bool active )
        {
			if (SCANButton.Visible != active)
				SCANButton.Visible = active;
			if (MapButton.Visible != active)
				MapButton.Visible = active;
			if (SmallButton.Visible != active)
				SmallButton.Visible = active;
        }

		private void toggleMenu ( IButton menu )
        {
			if (menu.Drawable == null)
				createMenu (menu);
			else
				destroyMenu (menu);
        }

		private void createMenu ( IButton menu )
        {
			PopupMenuDrawable list = new PopupMenuDrawable ();
			IButton smallMap = list.AddOption ("Small Map");
			IButton bigMap = list.AddOption ("Big Map");
			IButton instrument = list.AddOption ("Instruments");
			IButton settings = list.AddOption ("Settings");
			smallMap.OnClick += (e2) => SCANui.minimode = (SCANui.minimode == 0 ? 2 : -SCANui.minimode);
			bigMap.OnClick += (e2) => SCANui.bigmap_visible = !SCANui.bigmap_visible;
			instrument.OnClick += (e2) => SCANui.instruments_visible = !SCANui.instruments_visible;
			settings.OnClick += (e2) => SCANui.settings_visible = !SCANui.settings_visible;
			list.OnAnyOptionClicked += (  ) => destroyMenu (menu);
			menu.Drawable = list;
        }

		private void destroyMenu ( IButton menu )
        {
			((PopupMenuDrawable)menu.Drawable).Destroy ();
			menu.Drawable = null;
        }

		internal void OnDestroy ()
        {
			SCANButton.Destroy ();
			MapButton.Destroy ();
			SmallButton.Destroy ();
        }

    }       
}
