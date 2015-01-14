/*  KSPAddonImproved by xEvilReeperx
 * 
 *  - Allows definition of multiple startup scenes
 *  
 * http://forum.kerbalspaceprogram.com/threads/79889-Expanded-KSPAddon-modes?p=1157014&viewfull=1#post1157014
 *  
 * Provided in the Public Domain
 * 
 */

using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace SCANsat.SCAN_Platform
{

	[AttributeUsage(AttributeTargets.Class)]
	internal class SCAN_KSPAddonImproved : Attribute
	{
		[Flags]
		public enum Startup
		{
			// KSPAddon.Startup values:
			/*  Instantly = -2,
				EveryScene,
				EditorAny = -3,
				MainMenu = 2,
				Settings,
				SpaceCentre = 5,
				Credits = 4,
				EditorVAB = 6,
				EditorSPH = 9,
				Flight = 7,
				TrackingStation,
				PSystemSpawn = 10
			*/

			None = 0,
			MainMenu = 1 << 0,
			Settings = 1 << 1,
			SpaceCenter = 1 << 2,
			Credits = 1 << 3,
			EditorVAB = 1 << 4,
			//EditorSPH = 1 << 5,
			Flight = 1 << 5,
			TrackingStation = 1 << 6,
			PSystemSpawn = 1 << 7,
			Instantly = 1 << 8,

			TimeElapses = Flight | TrackingStation | SpaceCenter,
			RealTime = TimeElapses,
			EveryScene = ~0
		}

		public bool runOnce;
		public Startup scenes;

		public SCAN_KSPAddonImproved(Startup mask, bool once = false)
		{
			runOnce = once;
			scenes = mask;
		}
	}

	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	internal class CustomAddonLoader : MonoBehaviour
	{
		// What's improved? The KSPAddon.Startup is now a bitmask so you can
		// use logical operations to specify which scenes you want your addon
		// to be loaded in


		// master list to keep track of addons in our assembly
		List<AddonInfo> addons = new List<AddonInfo>();
		private string _identifier;

		// Mainly required so we can flag addons when they've
		// been created in the case of runOnce = true
		class AddonInfo
		{
			public readonly Type type;
			public readonly SCAN_KSPAddonImproved addon;
			public bool created;

			internal AddonInfo(Type t, SCAN_KSPAddonImproved add)
			{
				type = t;
				created = false;

				addon = add;
			}

			internal bool RunOnce
			{
				get
				{
					return addon.runOnce;
				}
			}

			internal SCAN_KSPAddonImproved.Startup Scenes
			{
				get
				{
					return addon.scenes;
				}
			}
		}



		void Awake()
		{
			DontDestroyOnLoad(this);

			// multiple plugins using this source will create their own instances
			// of the loader; the log can get confusing pretty fast without some
			// way of telling them apart
			_identifier = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + "." + GetType().ToString();

			// examine our assembly for loaded types
			foreach (var ourType in System.Reflection.Assembly.GetExecutingAssembly().GetTypes())
			{
				var attr = ((SCAN_KSPAddonImproved[])ourType.GetCustomAttributes(typeof(SCAN_KSPAddonImproved), true)).SingleOrDefault();
				if (attr != null)
				{
					Debug.Log(string.Format("Found KSPAddonImproved in {0}", ourType.FullName));
					addons.Add(new AddonInfo(ourType, attr));
				}
			}

			// special case here: since we're already in the first scene,
			// OnLevelWasLoaded won't be invoked so we need to fire off any
			// "instant" loading addons now
			OnLevelWasLoaded((int)GameScenes.LOADING);
		}



		void OnLevelWasLoaded(int level)
		{
			GameScenes scene = (GameScenes)level;
			SCAN_KSPAddonImproved.Startup mask = 0;

			if (scene == GameScenes.LOADINGBUFFER)
				return;

			Debug.Log(string.Format("{1}: {0} was loaded; instantiating addons...", scene.ToString(), _identifier));

			// Convert GameScenes => SceneMask
			switch (scene)
			{
				case GameScenes.EDITOR:
					mask = SCAN_KSPAddonImproved.Startup.EditorVAB;
					break;

				case GameScenes.CREDITS:
					mask = SCAN_KSPAddonImproved.Startup.Credits;
					break;

				case GameScenes.FLIGHT:
					mask = SCAN_KSPAddonImproved.Startup.Flight;
					break;

				case GameScenes.LOADING:
					mask = SCAN_KSPAddonImproved.Startup.Instantly;
					break;

				case GameScenes.MAINMENU:
					mask = SCAN_KSPAddonImproved.Startup.MainMenu;
					break;

				case GameScenes.SETTINGS:
					mask = SCAN_KSPAddonImproved.Startup.Settings;
					break;

				case GameScenes.SPACECENTER:
					mask = SCAN_KSPAddonImproved.Startup.SpaceCenter;
					break;

				case GameScenes.TRACKSTATION:
					mask = SCAN_KSPAddonImproved.Startup.TrackingStation;
					break;

				case GameScenes.PSYSTEM:
					mask = SCAN_KSPAddonImproved.Startup.PSystemSpawn;
					break;

				case GameScenes.LOADINGBUFFER:
					// intentionally left unset
					break;

				default:
					Debug.LogError(string.Format("{1} unrecognized scene: {0}", scene.ToString(), _identifier));
					break;
			}

			int counter = 0;

			for (int i = 0; i < addons.Count; ++i)
			{
				var addon = addons[i];

				if (addon.created && addon.RunOnce)
					continue; // this addon was already loaded

				// should this addon be initialized in current scene?
				if ((addon.Scenes & mask) != 0)
				{
					Debug.Log(string.Format("ImprovedAddonLoader: Creating addon '{0}'", addon.type.Name));
					GameObject go = new GameObject(addon.type.Name);
					go.AddComponent(addon.type);

					addon.created = true;
					++counter;
				}
			}

			Debug.Log(string.Format("{1} finished; created {0} addons", counter, _identifier));
		}
	}
}