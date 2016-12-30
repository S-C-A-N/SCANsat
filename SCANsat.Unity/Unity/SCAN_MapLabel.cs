using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SCANsat.Unity.Unity
{
	public class SCAN_MapLabel : MonoBehaviour
	{
		[SerializeField]
		private Image m_Image = null;
		[SerializeField]
		private TextHandler m_Label = null;

		private Guid _id;
		private RectTransform rect;

		public Guid ID
		{
			get { return _id; }
		}

		private void Awake()
		{
			rect = GetComponent<RectTransform>();
		}

		public void Setup(Guid id, string label, Sprite image, Vector2 pos)
		{
			_id = id;

			UpdateLabel(label);

			UpdateImage(image);

			UpdatePosition(pos);
		}

		public void UpdateLabel(string l)
		{
			if (!string.IsNullOrEmpty(l) && m_Label != null)
				m_Label.OnTextUpdate.Invoke(l);
		}

		public void UpdateImage(Sprite s)
		{
			if (m_Image != null)
				m_Image.sprite = s;
		}

		public void UpdatePosition(Vector2 p)
		{
			if (rect != null)
				rect.anchoredPosition = p;
		}

	}
}
