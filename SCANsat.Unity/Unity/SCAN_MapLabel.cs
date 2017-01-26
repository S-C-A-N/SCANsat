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
		[SerializeField]
		private LayoutElement m_Layout = null;

		private Guid _guid;
		private int _intID;
		private string _stringID;
		private RectTransform rect;
		private MapLabelInfo label;

		private int updateInterval = 60;
		private int lastUpdate;
		private bool flip;

		public Guid ID
		{
			get { return _guid; }
		}

		public int IntID
		{
			get { return _intID; }
		}

		public string StringID
		{
			get { return _stringID; }
		}

		private void Awake()
		{
			rect = GetComponent<RectTransform>();
		}

		private void Update()
		{
			if (!label.flash || m_Image == null || m_Label == null)
				return;

			lastUpdate++;

			if (lastUpdate < updateInterval)
				return;

			lastUpdate = 0;
			flip = !flip;

			if (flip)
			{
				m_Image.color = label.flashColor;
				m_Label.OnColorUpdate.Invoke(label.flashColor);
			}
			else
			{
				m_Image.color = label.baseColor;
				m_Label.OnColorUpdate.Invoke(label.baseColor);
			}
		}

		public void Setup(Guid id, MapLabelInfo info)
		{
			_guid = id;

			Setup(info);
		}

		public void Setup(int id, MapLabelInfo info)
		{
			_intID = id;
			
			Setup(info);
		}

		public void Setup(string id, MapLabelInfo info)
		{
			_stringID = id;

			Setup(info);
		}

		public void Setup(MapLabelInfo info)
		{
			label = info;

			if (m_Image != null)
				m_Image.color = info.baseColor;

			if (m_Label != null)
				m_Label.OnColorUpdate.Invoke(info.baseColor);

			if (rect != null)
				rect.anchoredPosition = new Vector2(rect.anchoredPosition.x - (info.width / 2), info.alignBottom ? rect.anchoredPosition.y + (info.width / 2) : rect.anchoredPosition.y);

			UpdateLabel(info.label);

			UpdateImage(info.image);

			UpdatePosition(info.pos);

			UpdateSize(info.width);
		}

		public void UpdateLabel(string l)
		{
			if (m_Label != null)
				m_Label.OnTextUpdate.Invoke(l);
		}

		public void UpdateImage(Sprite s)
		{
			if (m_Image != null)
				m_Image.sprite = s;
		}

		public void UpdateSize(int i)
		{
			if (m_Layout != null)
			{
				m_Layout.preferredHeight = i;
				m_Layout.preferredWidth = i;
			}
		}

		public void UpdatePosition(Vector2 p)
		{
			if (rect != null)
				rect.anchoredPosition = new Vector2(p.x - (label.width / 2), label.alignBottom ? p.y + (label.width / 2) : p.y);
		}

	}
}
