using System;
using System.Collections.Generic;

using UnityEngine;
using Log = SCANsat.Platform.Logging.ConsoleLogger;


namespace UnityEngine {
	public static class GUIStyle_ {
		internal static List<GUIStyle> knownStyles { get; set; }
		
		public static GUIStyle dumpStyle(this GUIStyle s, string breadcrumbs) {
			var hash = s.GetHashCode ();
			
			if (knownStyles.Contains(s)) { Log.Now ("{0} => #[{2:X8}] {1}  skipped",breadcrumbs,s.ToString(), hash); return s; }
			

			
			Log.Now ("{0} => #[{2:X8}] {1}", breadcrumbs, s.ToString(),s.GetHashCode ());
			Log.Now ("             ------------------------------------------------------------------------------------------------ ");
			Log.Now ("              -> contentOffset => {0}",s.contentOffset);
			Log.Now ("             ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ ");
			Log.Now ("              -> border    => {0}",s.border);
			Log.Now ("              -> padding   => {0}",s.border);
			Log.Now ("              -> margin    => {0}",s.border);
			Log.Now ("              -> overflow  => {0}",s.border);
			// print all of the non-trivialGUIStyleState components
			Log.Now ("             ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ ");
			s.active.PPP	("              -> active    => ");
			s.onActive.PPP	("              -> onActive  => ");
			s.normal.PPP	("              -> normal    => ");
			s.onNormal.PPP	("              -> onNormal  => ");
			s.hover.PPP		("              -> hover     => ");
			s.onHover.PPP	("              -> onHover   => ");
			s.focused.PPP	("              -> focused   => ");
			s.onFocused.PPP	("              -> onFocused => ");
			Log.Now ("             ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ ");
			Log.Now ("              -> alignment => {0}",s.alignment);
			Log.Now ("              -> wordwrap  => {0}",s.alignment);
			Log.Now ("              -> clipping  => {0}",s.clipping);
			Log.Now ("              -> richText? => {0}",s.richText);
			Log.Now ("              -> lineHeight=> {0}",s.lineHeight);
			Log.Now ("              -> font      => {0}",s.fontPP ());
			Log.Now ("              -> fontSize  => {0}",s.fontSize);
			Log.Now ("              -> fontStyle => {0}",s.fontStyle);
			Log.Now ("             ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ ");
			Log.Now ("              -> fixedW, stretchW?  => {0} ({1})",s.fixedWidth, s.stretchWidth);
			Log.Now ("              -> fixedH, stretchH?  => {0} ({1})",s.fixedHeight,s.stretchHeight);
			Log.Now ("              -> isHdepW? => {0}",s.isHeightDependantOnWidth);
			Log.Now ("             ------------------------------------------------------------------------------------------------ ");
			
			return s;
		}
		
		
		public static string fontPP(this GUIStyle s) {
			string font = "null";
			if (s.font != null) font = s.font.ToString ();
			return font;
		}
		
		public static GUIStyle PaddingChange(this GUIStyle g, Int32 PaddingValue)
		{
			GUIStyle gReturn = new GUIStyle(g);
			gReturn.padding = new RectOffset(PaddingValue, PaddingValue, PaddingValue, PaddingValue);
			return gReturn;
		}
		public static GUIStyle PaddingChangeBottom(this GUIStyle g, Int32 PaddingValue)
		{
			GUIStyle gReturn = new GUIStyle(g);
			gReturn.padding.bottom = PaddingValue;
			return gReturn;
		}
	}
}

