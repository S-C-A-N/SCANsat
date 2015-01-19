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

using System;
using System.Collections;
using SCANsat.SCAN_Platform;
using SCANsat.SCAN_UI;
using SCANsat.SCAN_UI.UI_Framework;

namespace SCANsat.SCAN_Toolbar
{
	class SCANappLauncher : SCAN_MBE
	{
		private ApplicationLauncherButton SCANappLauncherButton = null;

		internal override void Start()
		{
			setupToolbar();
		}

		internal override void OnDestroy()
		{
			GameEvents.onGUIApplicationLauncherUnreadifying.Remove(removeButton);

			if (SCANappLauncherButton != null)
				removeButton(HighLogic.LoadedScene);
		}

		private void setupToolbar()
		{
			SCANUtil.SCANdebugLog("Starting App Launcher Manager");
			StartCoroutine(addButton());
		}

		IEnumerator addButton()
		{
			SCANUtil.SCANdebugLog("Waiting For Application Launcher...");

			while (!ApplicationLauncher.Ready)
				yield return null;

			if (HighLogic.LoadedScene == GameScenes.FLIGHT)
				SCANappLauncherButton = ApplicationLauncher.Instance.AddModApplication(toggleFlightOn, toggleFlightOff, null, null, null, null, ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW, SCANskins.SCAN_SmallMapAppIcon);
			else if (HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedScene == GameScenes.TRACKSTATION)
				SCANappLauncherButton = ApplicationLauncher.Instance.AddModApplication(toggleKSCOn, toggleKSCOff, null, null, null, null, ApplicationLauncher.AppScenes.SPACECENTER | ApplicationLauncher.AppScenes.TRACKSTATION, SCANskins.SCAN_BigMapAppIcon);

			GameEvents.onGUIApplicationLauncherUnreadifying.Add(removeButton);
		}

		private void removeButton(GameScenes scene)
		{
			ApplicationLauncher.Instance.RemoveModApplication(SCANappLauncherButton);
			SCANappLauncherButton = null;
			SCANUtil.SCANdebugLog("App Launcher Button Removed");
		}

		private void toggleFlightOn()
		{
			if (SCANcontroller.controller != null)
			{
				SCANcontroller.controller.mainMap.Visible = true;
				SCANcontroller.controller.mainMapVisible = true;
			}
		}

		private void toggleFlightOff()
		{
			if (SCANcontroller.controller != null)
			{
				SCANcontroller.controller.mainMap.Visible = false;
				SCANcontroller.controller.mainMapVisible = false;
			}
		}

		private void toggleKSCOn()
		{
			if (SCANcontroller.controller != null)
			{
				SCANcontroller.controller.kscMap.Visible = true;
				SCANcontroller.controller.kscMapVisible = true;
			}
		}

		private void toggleKSCOff()
		{
			if (SCANcontroller.controller != null)
			{
				SCANcontroller.controller.kscMap.Visible = false;
				SCANcontroller.controller.kscMapVisible = false;
			}
		}
	}
}
