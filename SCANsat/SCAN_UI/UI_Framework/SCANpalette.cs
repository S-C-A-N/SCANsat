#region license
/*
 * [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 * 
 * SCANpalette - manages colors and palettes of colors
 *
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
*/
#endregion
using System;
//using System.Collections.Generic;
//using System.Text.RegularExpressions;
using UnityEngine;
using SCANsat.Platform.Palettes;

namespace SCANsat.SCAN_UI
{
	public class SCANpalette
	{

		// Basic Colors
		// 	(these are here just so that all references to some color are the same throughout
		//		SCANsat)
		public static Color black 	= Color.black;
		public static Color white 	= Color.white;
		public static Color red		= Color.red;
		public static Color grey		= Color.grey;
		public static Color clear	= Color.clear;
		public static Color magenta	= Color.magenta;
		public static Color yellow	= Color.yellow;
		public static Color cyan		= Color.cyan;
		public static Color blue		= Color.blue;
		public static Color green	= Color.green;

		public static Color lerp (Color a, Color b, float t) {
			return Color.Lerp (a,b,t);
		}

		// XKCD Colors
		// 	(these are collected here for the same reason)

		public static Color xkcd_Amber		= XKCDColors.Amber;
		public static Color xkcd_ArmyGreen		= XKCDColors.ArmyGreen;
		public static Color xkcd_PukeGreen		= XKCDColors.PukeGreen;
		public static Color xkcd_Lemon		= XKCDColors.Lemon;
		public static Color xkcd_OrangeRed 	= XKCDColors.OrangeRed;
		public static Color xkcd_CamoGreen		= XKCDColors.CamoGreen;
		public static Color xkcd_Marigold		= XKCDColors.Marigold;
		public static Color xkcd_Puce			= XKCDColors.Puce;
		public static Color xkcd_DarkTeal 		= XKCDColors.DarkTeal;
		public static Color xkcd_DarkPurple	= XKCDColors.DarkPurple;
		public static Color xkcd_DarkGrey		= XKCDColors.DarkGrey;
		public static Color xkcd_LightGrey		= XKCDColors.LightGrey;
		public static Color xkcd_PurplyPink	= XKCDColors.PurplyPink;
		public static Color xkcd_Magenta		= XKCDColors.Magenta;
		public static Color xkcd_YellowGreen	= XKCDColors.YellowGreen;
		public static Color xkcd_LightRed		= XKCDColors.LightRed;
		public static Color xkcd_Cerulean		= XKCDColors.Cerulean;
		public static Color xkcd_Yellow		= XKCDColors.Yellow;
		public static Color xkcd_Red			= XKCDColors.Red;
		public static Color xkcd_White		= XKCDColors.White;


		// colourblind barrier-free colours, according to Masataka Okabe and Kei Ito
		// http://jfly.iam.u-tokyo.ac.jp/color/

		public static Color cb_skyBlue 		= new Color(0.35f, 0.7f, 0.9f);		// sky blue
		public static Color cb_bluishGreen 	= new Color(0f, 0.6f, 0.5f);		// bluish green
		public static Color cb_yellow 		= new Color(0.95f, 0.9f, 0.25f);	// yellow
		public static Color cb_blue 			= new Color(0f, 0.45f, 0.7f);		// blue
		public static Color cb_vermillion 		= new Color(0.8f, 0.4f, 0f);		// vermillion
		public static Color cb_reddishPurple	= new Color(0.8f, 0.6f, 0.7f);		// reddish purple
		public static Color cb_orange 		= new Color(0.9f, 0.6f, 0f);		// orange

		/* SOLARIZED colors: currently unused, so commented out */
		/*
		public static Color sol_base03 	= new Color32(0,43,54,255);
		public static Color sol_base02 	= new Color32(7,54,66,255);
		public static Color sol_base01 	= new Color32(88,110,117,255);
		public static Color sol_base00 	= new Color32(101,123,131,255);
		public static Color sol_base0 	= new Color32(131,148,150,255);
		public static Color sol_base1 	= new Color32(147,161,161,255);
		public static Color sol_base2 	= new Color32(238,232,213,255);
		public static Color sol_base3 	= new Color32(253,246,227,255);
		public static Color sol_yellow 	= new Color32(181,137,0,255);
		public static Color sol_orange 	= new Color32(203,75,22,255);
		public static Color sol_red 		= new Color32(45,220,50,255);
		public static Color sol_magenta 	= new Color32(211,54,130,255);
		public static Color sol_violet 	= new Color32(108,113,196,255);
		public static Color sol_blue 		= new Color32(38,139,210,255);
		public static Color sol_cyan 		= new Color32(42,161,152,255);
		public static Color sol_green 	= new Color32(133,153,0,255);
		*/
		public static Color[] heightGradient = {
			xkcd_ArmyGreen,
			xkcd_Yellow,
			xkcd_Red,
			xkcd_Magenta,
			xkcd_White,
			xkcd_White
		};

		private static Palette currentHeightPalette;

		public static Color[] redline;
        	public static Color gridFull; // resource colors
        	public static Color gridEmpty; //empty resource color

		public static Color[] small_redline;

		public static Color heightToColor(float val, int scheme, SCANdata data)
		{
			float range = data.MaxHeight - data.MinHeight;
			if (scheme == 1 || SCANcontroller.controller.colours == 1)
			{
				return lerp(black, white, Mathf.Clamp((val - data.MinHeight) / range, 0, 1));
			}
			Color c = black;
			if (data.ClampHeight != null)
			{
				if (val <= (float)data.ClampHeight)
				{
					val = (Mathf.Clamp(val, data.MinHeight, (float)data.ClampHeight) - data.MinHeight / ((float)data.ClampHeight - data.MinHeight));
					c = lerp(data.ColorPalette.colors[0], data.ColorPalette.colors[1], val);
				}
			}
			else
			{
				val -= data.MinHeight;
				val = (data.ColorPalette.colors.Length - 1) * Mathf.Clamp(val, 0, range) / range;
				if ((int)val > data.ColorPalette.colors.Length - 2) val = data.ColorPalette.colors.Length - 2;
				c = lerp(data.ColorPalette.colors[(int)val], data.ColorPalette.colors[(int)val + 1], val - (int)val);
			}

			//int sealevel = 0;
			//if (val <= sealevel) {
			//	val = (Mathf.Clamp (val , -1500 , sealevel) + 1500) / 1000f;
			//	c = lerp (xkcd_DarkPurple , xkcd_Cerulean , val);
			//} else {
			//	val = (heightGradient.Length - 2) * Mathf.Clamp (val , sealevel , (sealevel + 7500)) / (sealevel + 7500.0f);
			//	c = lerp (heightGradient [(int)val] , heightGradient [(int)val + 1] , val - (int)val);
			//}
			return c;
		}

		public static string colorHex ( Color32 c ) {
			return "#" + c.r.ToString ("x2") + c.g.ToString ("x2") + c.b.ToString ("x2");
		}
		public static string colored ( Color c , string text ) {
			return "<color=\"" + colorHex (c) + "\">" + text + "</color>";
		}

		internal static Color c_good {
			get {
				if (SCANcontroller.controller.colours != 1) 	return xkcd_PukeGreen;
				else 								return cb_skyBlue;
			}
		}
		internal static Color c_bad
		{
			get
			{
				return cb_orange;
			}
		}
		internal static Color c_ugly {
			get {
				if (SCANcontroller.controller.colours != 1)	return xkcd_LightRed;
				else 								return cb_yellow;
			}
		}


		public static Color picker(Rect r, Color c) {


			GUILayout.BeginArea (r,"","Box");
			GUILayout.BeginHorizontal ();
			// R
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal ();
			// G
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal ();
			// B
			GUILayout.EndHorizontal();

			GUILayout.EndArea ();
			return c;
		}

		public static void swatch(Color c) {
			Texture2D tex = new Texture2D (20,20);
			GUILayout.BeginVertical ("Box",new GUILayoutOption[]{GUILayout.Width (22),GUILayout.Height (22)});
			GUI.color = c;
			GUILayout.Label (tex);
			GUI.color = Color.white;
			GUILayout.EndVertical ();
		}

		private static _Palettes currentPaletteSet;
		private static string currentPaletteType;

		internal static _Palettes generatePaletteSet(int size, Palette.Kind type)
		{
			PaletteLoader.generatePalettes(type, size);
			return new _Palettes(PaletteLoader.palettes.ToArray(), type);
		}

		internal static _Palettes CurrentPalettes
		{
			get { return currentPaletteSet; }
			set
			{
				currentPaletteSet = value;
				currentPaletteType = value.paletteType.ToString();
			}
		}

		internal static string getPaletteType
		{
			get { return currentPaletteType; }
			private set { }
		}

		internal static Palette CurrentPalette
		{
			get { return currentHeightPalette; }
			set { currentHeightPalette = value; }
		}


	}


}

