

using UnityEngine;

namespace SCANsat.SCAN_UI.UI_Framework
{
	public class SCANuiSlider
	{
		private float minValue, maxValue, currentValue;
		private float oldValue;
		private string title, units;

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

		public float CurrentValue
		{
			get { return currentValue; }
			internal set { currentValue = value; }
		}

		public SCANuiSlider(float Min, float Max, float Value, string Title, string Units)
		{
			minValue = Min;
			maxValue = Max;
			currentValue = Value;
			oldValue = Value;
			title = Title;
			units = Units;
		}

		public void drawSlider()
		{
			GUILayout.Label(title + currentValue + units, SCANskins.SCAN_whiteReadoutLabel);

			Rect r = GUILayoutUtility.GetLastRect();
			r.x += 110;
			r.width = 130;

			currentValue = GUI.HorizontalSlider(r, currentValue, minValue, maxValue).Mathf_Round(-2);

			SCANuiUtil.drawSliderLabel(r, minValue + units, maxValue + units);
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
