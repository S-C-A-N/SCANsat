using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SCANsat.SCAN_UI.UI_Framework
{
	public class SCANuiColorPicker
	{
		private float valSlider, oldValSlider;
		private bool lowColorChange, oldColorState;
		private int bodyIndex;
		private Color c = new Color();
		private Color colorLow = new Color();
		private Color colorHigh = new Color();
		private Texture2D minColorPreview = new Texture2D(1, 1);
		private Texture2D minColorOld = new Texture2D(1, 1);
		private Texture2D maxColorPreview = new Texture2D(1, 1);
		private Texture2D maxColorOld = new Texture2D(1, 1);

		internal SCANuiColorPicker (Color low, Color high)
		{
			colorLow = low;
			colorHigh = high;
		}

		internal void drawColorSelector(Rect R)
		{
			GUILayout.Space(30);
			GUILayout.BeginVertical();
				GUILayout.Label("Color Selection", SCANskins.SCAN_headline);
				GUILayout.BeginHorizontal();
					GUILayout.Space(30);
					GUILayout.Label(SCANskins.SCAN_BigColorWheel);
					Rect r = GUILayoutUtility.GetLastRect();
				GUILayout.EndHorizontal();
			GUILayout.EndVertical();

			valSlider = GUI.VerticalSlider(new Rect(280, 60, 30, 200), valSlider, 100, 0, SCANskins.SCAN_vertSlider, SCANskins.SCAN_sliderThumb).Mathf_Round(0);

			if (GUI.RepeatButton(r, "", SCANskins.SCAN_colorWheelButton))
			{
				int a = (int)Input.mousePosition.x;
				int b = Screen.height - (int)Input.mousePosition.y;

				c = SCANskins.SCAN_BigColorWheel.GetPixel(a - (int)R.x - (int)r.x, -(b - (int)R.y - (int)r.y));

				if (lowColorChange)
					colorLow = c * new Color(valSlider / 100f, valSlider / 100f, valSlider / 100f);
				else
					colorHigh = c * new Color(valSlider / 100f, valSlider / 100f, valSlider / 100f);
			}

			r.x -= 55;
			r.y += 145;
			r.width = 60;
			r.height = 30;

			colorSwatches(r, "Low", ref lowColorChange, true, minColorPreview, minColorOld, colorLow);

			r.x += 150;
			colorSwatches(r, "High", ref lowColorChange, false, maxColorPreview, maxColorOld, colorHigh);

			r.x -= 60;
			r.y += 30;
			GUI.Label(r, "New", SCANskins.SCAN_headlineSmall);

			r.y += 32;
			GUI.Label(r, "Old", SCANskins.SCAN_headlineSmall);
		}

		private void colorSwatches(Rect R, string Title, ref bool Active, bool Low, Texture2D Preview, Texture2D Current, Color New)
		{
			bool active;
			if (Low)
				active = Active;
			else
				active = !Active;

			active = GUI.Toggle(R, active, Title);

			if (Low)
				Active = active;
			else
				Active = !active;

			R.x += 10;
			R.y += 30;

			Preview.SetPixel(0, 0, New);
			Preview.Apply();
			GUI.DrawTexture(R, Preview);

			R.y += 32;
			GUI.DrawTexture(R, Current);
		}

		internal void brightnessChanged()
		{

		}
	}
}
