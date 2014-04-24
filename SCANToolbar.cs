/* Copyright (c) 2014 David Grandy <david.grandy@gmail.com>
 * SCANSat for Blizzy78's toolbar
 * 
 * Created by me to allow the SCANsat plugin to function through the toolbar interface

Redistribution and use in source and binary forms, with or without modifica-
tion, are permitted provided that the following conditions are met:

  1.  Redistributions of source code must retain the above copyright notice,
      this list of conditions and the following disclaimer.

  2.  Redistributions in binary form must reproduce the above copyright
      notice, this list of conditions and the following disclaimer in the
      documentation and/or other materials provided with the distribution.

  3.  The name of the author may not be used to endorse or promote products
      derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR IMPLIED
WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MER-
CHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO
EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPE-
CIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTH-
ERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED
OF THE POSSIBILITY OF SUCH DAMAGE.

*/

using UnityEngine;
using Toolbar;

namespace SCANsat
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    class SCANToolbar : MonoBehaviour
    {
        private IButton SCANButton;
        private IButton MapButton;
        private IButton SmallButton;

        internal SCANToolbar()
        {
            SCANButton = ToolbarManager.Instance.add("SCANsat", "UIMenu");           
            SCANButton.TexturePath = "SCANsat/SCANsat_Icon";
            SCANButton.ToolTip = "SCANsat";
            SCANButton.OnClick += (e) => toggleMenu(SCANButton);

            MapButton = ToolbarManager.Instance.add("SCANsat", "BigMap");
            //Map texture from SCANsat map created from in-game biome map of Kerbin
            MapButton.TexturePath = "SCANsat/SCANsat_Map_Icon";
            MapButton.ToolTip = "SCANsat Big Map";
            MapButton.OnClick += (e) => SCANui.bigmap_visible = !SCANui.bigmap_visible;

            SmallButton = ToolbarManager.Instance.add("SCANsat", "SmallMap");
            //The toolbar icon texture was pulled out of the unity package and edited by me to match the old SCANsat color scheme
            SmallButton.TexturePath = "SCANsat/SCANsat_SmallMap_Icon";
            SmallButton.ToolTip = "SCANsat Small Map";
            SmallButton.OnClick += (e) => SCANui.minimode = (SCANui.minimode == 0 ? 2 : -SCANui.minimode);
        }

        private void LateUpdate()
        {
            buttonVisible(FlightGlobals.ActiveVessel.FindPartModulesImplementing<SCANsat>().Count > 0);
        }

        private void buttonVisible(bool active)
        {
            if (SCANButton.Visible != active) SCANButton.Visible = active;
            if (MapButton.Visible != active) MapButton.Visible = active;
            if (SmallButton.Visible != active) SmallButton.Visible = active;
        }

        private void toggleMenu(IButton menu)
        {
            if (menu.Drawable == null) createMenu(menu);
            else destroyMenu(menu);
        }

        private void createMenu(IButton menu)
        {
            PopupMenuDrawable list = new PopupMenuDrawable();
            IButton smallMap 	= list.AddOption("Small Map");
            IButton bigMap 		= list.AddOption("Big Map");
            IButton instrument 	= list.AddOption("Instruments");
            IButton settings 	= list.AddOption("Settings");
            smallMap.OnClick 		+= (e2) => SCANui.minimode 				= (SCANui.minimode == 0 ? 2 : -SCANui.minimode);
            bigMap.OnClick 			+= (e2) => SCANui.bigmap_visible 		= !SCANui.bigmap_visible;
            instrument.OnClick 		+= (e2) => SCANui.instruments_visible 	= !SCANui.instruments_visible;
            settings.OnClick 		+= (e2) => SCANui.settings_visible 		= !SCANui.settings_visible;
            list.OnAnyOptionClicked += (  ) => destroyMenu(menu);
            menu.Drawable = list;
        }

        private void destroyMenu(IButton menu)
        {
            ((PopupMenuDrawable) menu.Drawable).Destroy();
            menu.Drawable = null;
        }

        internal void OnDestroy()
        {
            SCANButton.Destroy();
            MapButton.Destroy();
            SmallButton.Destroy();
        }

    }       
}
