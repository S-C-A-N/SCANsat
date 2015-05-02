#region license
/* 
 * [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 *
 * SCANuiSlider - object to handle drawing and labeling GUI sliders
 * 
 * Copyright (c)2014 David Grandy <david.grandy@gmail.com>;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
 */
#endregion

using UnityEngine;

namespace SCANsat.SCAN_UI.UI_Framework
{
	public class SCANuiSlider
	{
		private float minValue, maxValue, currentValue;
		private float oldValue;
		private string title, units;
		private int precision;

		public float MinValue
		{
			get { return minValue; }
			internal set
			{
				if (value < maxValue)
					minValue = value;
			}
		}

		public float MaxValue
		{
			get { return maxValue; }
			internal set
			{
				if (value > minValue)
					maxValue = value;
			}
		}

		//public float CurrentValue
		//{
		//	get { return currentValue; }
		//	internal set { currentValue = value; }
		//}

		public SCANuiSlider(float Min, float Max, float Value, string Title, string Units, int Prec)
		{
			minValue = Min;
			maxValue = Max;
			currentValue = Value;
			oldValue = Value;
			title = Title;
			units = Units;
			precision = Prec;
		}

		public float drawSlider(bool under, ref float value)
		{
			GUILayout.Label(title + value + units, SCANskins.SCAN_whiteReadoutLabel);

			Rect r = GUILayoutUtility.GetLastRect();
			r.x += 110;
			r.width = 260;

			if (under)
				GUI.HorizontalSlider(r, value, minValue, maxValue).Mathf_Round(precision);
			else
				value = GUI.HorizontalSlider(r, value, minValue, maxValue).Mathf_Round(precision);

			int i = precision <= 0 ? 0 : precision;
			string labelPrecision = "F" + i.ToString();
			SCANuiUtil.drawSliderLabel(r, minValue.ToString(labelPrecision) + units, maxValue.ToString(labelPrecision) + units);

			currentValue = value;

			return value;
		}

		public bool valueChanged()
		{
			if (oldValue != currentValue)
			{
				oldValue = currentValue;
				return true;
			}

			return false;
		}
	}
}
