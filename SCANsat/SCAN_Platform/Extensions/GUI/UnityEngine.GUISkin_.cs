using System;
using System.Collections.Generic;
using SCANsat.SCAN_Platform;
using Log = SCANsat.SCAN_Platform.Logging.ConsoleLogger;


namespace UnityEngine
{
	public static class GUISkin_
	{
		public static void dumpSkins(this GUISkin s) {
			
			string msg= "";
			Object[] fonts = UnityEngine.Resources.FindObjectsOfTypeAll( typeof(UnityEngine.Font) );
			foreach (Object f in fonts) {
				msg += "found a font: " + ((UnityEngine.Font) f).name + "\n";
			}

			Log.Debug("----------------------------------\n{0}", msg);
			
			foreach (KeyValuePair<string,GUISkin> e in SCAN_SkinsLibrary.knownSkins) {
				string k = e.Key;
				int hash = e.GetHashCode ();
				GUISkin v = e.Value;
				GUIStyle[] cs = e.Value.customStyles;
				
				string prefix = "#[" + hash.ToString ("X8") + "]" + " GUISkin ";
				
				GUIStyle_.knownStyles.Add (v.box.dumpStyle(prefix + k + ".box"));
				GUIStyle_.knownStyles.Add (v.button.dumpStyle(prefix + k + ".button"));
				GUIStyle_.knownStyles.Add (v.label.dumpStyle(prefix + k + ".label"));
				GUIStyle_.knownStyles.Add (v.scrollView.dumpStyle(prefix + k + ".scrollView"));
				GUIStyle_.knownStyles.Add (v.textArea.dumpStyle(prefix + k + ".textArea"));
				GUIStyle_.knownStyles.Add (v.textField.dumpStyle(prefix + k + ".textField"));
				GUIStyle_.knownStyles.Add (v.toggle.dumpStyle(prefix + k + ".toggle"));
				GUIStyle_.knownStyles.Add (v.horizontalSlider.dumpStyle(prefix + k + ".HSlider"));
				GUIStyle_.knownStyles.Add (v.horizontalSliderThumb.dumpStyle(prefix + k + ".HSliderThumb"));
				GUIStyle_.knownStyles.Add (v.horizontalScrollbar.dumpStyle(prefix + k + ".HScrollbar"));
				GUIStyle_.knownStyles.Add (v.horizontalScrollbarLeftButton.dumpStyle(prefix + k + ".HScrollbarLeftButton"));
				GUIStyle_.knownStyles.Add (v.horizontalScrollbarRightButton.dumpStyle(prefix + k + ".HScrollbarRightButton"));
				GUIStyle_.knownStyles.Add (v.horizontalScrollbarThumb.dumpStyle(prefix + k + ".HScrollbarThumb"));
				GUIStyle_.knownStyles.Add (v.verticalSlider.dumpStyle(prefix + k + ".VSlider"));
				GUIStyle_.knownStyles.Add (v.verticalSliderThumb.dumpStyle(prefix + k + ".VSliderThumb"));
				GUIStyle_.knownStyles.Add (v.verticalScrollbar.dumpStyle(prefix + k + ".VScrollbar"));
				GUIStyle_.knownStyles.Add (v.verticalScrollbarUpButton.dumpStyle(prefix + k + ".VScrollbarLeftButton"));
				GUIStyle_.knownStyles.Add (v.verticalScrollbarDownButton.dumpStyle(prefix + k + ".VScrollbarRightButton"));
				GUIStyle_.knownStyles.Add (v.verticalScrollbarThumb.dumpStyle(prefix + k + ".VScrollbarThumb"));

				int i = 0;
				Log.Debug("GUISkin {0}.customStyles contains {1} custom styles:", k, cs.Length);
				foreach(GUIStyle sty in cs) {
					string csi = ".customStyles[" + i++ + "]";
					GUIStyle_.knownStyles.Add (sty.dumpStyle(prefix + k + csi));
				}
			}
		}
	}
}

