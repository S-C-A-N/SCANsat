#region license
/*
 * [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 * 
 * SCANappLauncher - A class to initialize and destroy the stock app launcher button
 *
 * Copyright (c)2013 damny;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
*/
#endregion

using System.Collections;
using SCANsat.SCAN_Platform;
using SCANsat.SCAN_UI;
using SCANsat.SCAN_UI.UI_Framework;

namespace SCANsat.SCAN_Toolbar
{
	class SCANappLauncher : SCAN_MBE
	{
		private ApplicationLauncherButton SCANappLauncherButton = null;

		protected override void Start()
		{
			setupToolbar();
		}

		protected override void OnDestroy()
		{
			GameEvents.onGUIApplicationLauncherUnreadifying.Remove(removeButton);

			if (SCANappLauncherButton != null)
				removeButton(HighLogic.LoadedScene);
		}

		private void setupToolbar()
		{
			StartCoroutine(addButton());
		}

		IEnumerator addButton()
		{
			while (!ApplicationLauncher.Ready)
				yield return null;

			if (HighLogic.LoadedScene == GameScenes.FLIGHT)
			{
				SCANappLauncherButton = ApplicationLauncher.Instance.AddModApplication(toggleFlight, toggleFlight, null, null, null, null, ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW, SCANskins.SCAN_SmallMapAppIcon);
			}
			else if (HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedScene == GameScenes.TRACKSTATION)
			{
				SCANappLauncherButton = ApplicationLauncher.Instance.AddModApplication(toggleKSC, toggleKSC, null, null, null, null, ApplicationLauncher.AppScenes.SPACECENTER | ApplicationLauncher.AppScenes.TRACKSTATION, SCANskins.SCAN_BigMapAppIcon);
			}

			GameEvents.onGUIApplicationLauncherUnreadifying.Add(removeButton);
		}

		private void removeButton(GameScenes scene)
		{
			if (SCANappLauncherButton != null)
			{
				ApplicationLauncher.Instance.RemoveModApplication(SCANappLauncherButton);
				SCANappLauncherButton = null;
			}
		}

		private void toggleFlight()
		{
			if (SCANcontroller.controller != null)
			{
				SCANcontroller.controller.mainMap.Visible = !SCANcontroller.controller.mainMap.Visible;
				SCANcontroller.controller.mainMapVisible = !SCANcontroller.controller.mainMapVisible;
			}
		}

		private void toggleKSC()
		{
			if (SCANcontroller.controller != null)
			{
				SCANcontroller.controller.kscMap.Visible = !SCANcontroller.controller.kscMap.Visible;
				SCANcontroller.controller.kscMapVisible = !SCANcontroller.controller.kscMapVisible;
			}
		}
	}
}
