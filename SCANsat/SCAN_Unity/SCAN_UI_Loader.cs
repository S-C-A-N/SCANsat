using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SCANsat.Unity.UISkin;
using SCANsat.Unity;
using SCANsat.Unity.Unity;
using TMPro;
using palette = SCANsat.SCAN_UI.UI_Framework.SCANpalette;

namespace SCANsat.SCAN_Unity
{
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	public class SCAN_UI_Loader : MonoBehaviour
	{
		private static bool loaded;
		private static bool skinLoaded;
		private static bool spritesLoaded;
		private static bool tmpProcessed;
		private static bool prefabsProcessed;
		private static bool clearLoaded;
		private static bool toggleLoaded;

		private static UISkinDef _unitySkinDef;

		private static Sprite _clearSprite;
		private static Sprite _toggleNormal;
		private static Sprite _toggleHover;
		private static Sprite _toggleActive;
		private static Sprite _toggleOn;

		private static GameObject[] loadedPrefabs;

		private static GameObject _mainMapPrefab;
		private static GameObject _bigMapPrefab;
		private static GameObject _instrumentsPrefab;
		private static GameObject _overlayPrefab;
		private static GameObject _settingsPrefab;

		public static GameObject MainMapPrefab
		{
			get { return _mainMapPrefab; }
		}

		public static GameObject BigMapPrefab
		{
			get { return _bigMapPrefab; }
		}

		public static GameObject InstrumentsPrefab
		{
			get { return _instrumentsPrefab; }
		}

		public static GameObject OverlayPrefab
		{
			get { return _overlayPrefab; }
		}

		public static GameObject SettingsPrefab
		{
			get { return _settingsPrefab; }
		}

		public static UISkinDef UnitySkinDef
		{
			get { return _unitySkinDef; }
		}

		public static void ResetUIStyle()
		{
			if (loadedPrefabs != null)
				processUIPrefabs();
		}

		private static string path;

		private void Awake()
		{
			if (loaded)
				Destroy(gameObject);

			path = KSPUtil.ApplicationRootPath + "GameData/SCANsat/Resources";

			if (!spritesLoaded)
				loadTextures();

			if (!skinLoaded)
				loadUnitySkin();

			loadPrefabBundle();

			loaded = true;
		}

		private static void loadTextures()
		{
			if (!clearLoaded)
			{
				Texture2D clear = new Texture2D(1, 1);
				clear.SetPixel(1, 1, palette.clear);
				clear.Apply();

				_clearSprite = Sprite.Create(clear, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));

				clearLoaded = true;
			}

			if (!toggleLoaded)
			{
				GUISkin skin = HighLogic.Skin;

				if (skin == null)
					return;

				_toggleNormal = Sprite.Create(skin.toggle.normal.background, new Rect(0, 0, skin.toggle.normal.background.width, skin.toggle.normal.background.height), new Vector2(0.5f, 0.5f));
				_toggleHover = Sprite.Create(skin.toggle.hover.background, new Rect(0, 0, skin.toggle.hover.background.width, skin.toggle.hover.background.height), new Vector2(0.5f, 0.5f));
				_toggleActive = Sprite.Create(skin.toggle.active.background, new Rect(0, 0, skin.toggle.active.background.width, skin.toggle.active.background.height), new Vector2(0.5f, 0.5f));
				_toggleOn = Sprite.Create(skin.toggle.onNormal.background, new Rect(0, 0, skin.toggle.onNormal.background.width, skin.toggle.onNormal.background.height), new Vector2(0.5f, 0.5f));
				
				toggleLoaded = true;
			}

			spritesLoaded = true;
		}

		private static void loadPrefabBundle()
		{
			if (loadedPrefabs == null)
			{
				AssetBundle prefabs = AssetBundle.LoadFromFile(path + "/scansat_prefabs.ksp");

				if (prefabs != null)
					loadedPrefabs = prefabs.LoadAllAssets<GameObject>();
			}

			if (loadedPrefabs != null)
			{
				if (!tmpProcessed)
					processTMPPrefabs();

				if (!prefabsProcessed)
					processUIPrefabs();
			}
		}

		private static void loadUnitySkin()
		{
			_unitySkinDef = new UISkinDef();

			SkinInit(_unitySkinDef);

			AssetBundle images = AssetBundle.LoadFromFile(path + "/scan_images.ksp");

			if (images == null)
				return;

			Sprite[] loadedImages = images.LoadAllAssets<Sprite>();

			if (loadedImages == null)
				return;

			for (int i = loadedImages.Length - 1; i >= 0; i--)
			{
				Sprite s = loadedImages[i];

				if (s.name == "window")
					_unitySkinDef.window.normal.background = s;
				else if (s.name == "box")
					_unitySkinDef.box.normal.background = s;
				else if (s.name == "button")
					_unitySkinDef.button.normal.background = s;
				else if (s.name == "button hover")
					_unitySkinDef.button.highlight.background = s;
				else if (s.name == "button on")
					_unitySkinDef.button.active.background = s;
				else if (s.name == "toggle")
					_unitySkinDef.toggle.normal.background = s;
				else if (s.name == "toggle hover")
					_unitySkinDef.toggle.highlight.background = s;
				else if (s.name == "toggle active")
					_unitySkinDef.toggle.active.background = s;
				else if (s.name == "SCAN_Toggle")
					_unitySkinDef.toggle.disabled.background = s;
				else if (s.name == "textfield")
					_unitySkinDef.textField.normal.background = s;
				else if (s.name == "textfield hover")
					_unitySkinDef.textField.highlight.background = s;
				else if (s.name == "textfield on")
					_unitySkinDef.textField.active.background = s;
				else if (s.name == "horizontal scrollbar")
					_unitySkinDef.horizontalScrollbar.normal.background = s;
				else if (s.name == "horizontal scrollbar thumb")
					_unitySkinDef.horizontalScrollbarThumb.normal.background = s;
				else if (s.name == "vertical scrollbar")
					_unitySkinDef.verticalScrollbar.normal.background = s;
				else if (s.name == "vertical scrollbar thumb")
					_unitySkinDef.verticalScrollbarThumb.normal.background = s;
				else if (s.name == "horizontalslider")
					_unitySkinDef.horizontalSlider.normal.background = s;
				else if (s.name == "slider thumb")
					_unitySkinDef.horizontalSliderThumb.normal.background = s;
				else if (s.name == "slider thumb hover")
					_unitySkinDef.horizontalSliderThumb.highlight.background = s;
				else if (s.name == "slider thumb active")
					_unitySkinDef.horizontalSliderThumb.active.background = s;
				else if (s.name == "verticalslider")
					_unitySkinDef.verticalSlider.normal.background = s;
				else if (s.name == "slider thumb")
					_unitySkinDef.verticalSliderThumb.normal.background = s;
				else if (s.name == "slider thumb hover")
					_unitySkinDef.verticalSliderThumb.highlight.background = s;
				else if (s.name == "slider thumb active")
					_unitySkinDef.verticalSliderThumb.active.background = s;
			}

			//AssetBundle skin = AssetBundle.LoadFromFile(path + "/scan_ghost.ksp");

			//if (skin == null)
			//	return;

			//SCAN_GhostSkin ghost = skin.LoadAsset<GameObject>("SCAN_Ghost_Skin").GetComponent<SCAN_GhostSkin>();

			//if (ghost == null)
			//	return;			

			//_unitySkinDef.name = ghost.GhostSkin.SkinName;

			//_unitySkinDef.window.name = ghost.GhostSkin.Window.Name;

			//_unitySkinDef.window.normal.background = ghost.GhostSkin.Window.Normal.Background;

			//_unitySkinDef.box.name = ghost.GhostSkin.Box.Name;
			//_unitySkinDef.box.normal.background = ghost.GhostSkin.Box.Normal.Background;

			//_unitySkinDef.button.name = ghost.GhostSkin.Button.Name;
			//_unitySkinDef.button.normal.background = ghost.GhostSkin.Button.Normal.Background;
			//_unitySkinDef.button.highlight.background = ghost.GhostSkin.Button.Highlight.Background;
			//_unitySkinDef.button.active.background = ghost.GhostSkin.Button.Active.Background;

			//_unitySkinDef.toggle.name = ghost.GhostSkin.Toggle.Name;
			//_unitySkinDef.toggle.normal.background = ghost.GhostSkin.Toggle.Normal.Background;
			//_unitySkinDef.toggle.highlight.background = ghost.GhostSkin.Toggle.Highlight.Background;
			//_unitySkinDef.toggle.active.background = ghost.GhostSkin.Toggle.Active.Background;
			//_unitySkinDef.toggle.disabled.background = ghost.GhostSkin.Toggle.CheckMark.Background;

			//_unitySkinDef.textField.name = ghost.GhostSkin.TextField.Name;
			//_unitySkinDef.textField.normal.background = ghost.GhostSkin.TextField.Normal.Background;
			//_unitySkinDef.textField.highlight.background = ghost.GhostSkin.TextField.Highlight.Background;
			//_unitySkinDef.textField.active.background = ghost.GhostSkin.TextField.Active.Background;

			//_unitySkinDef.horizontalScrollbar.name = ghost.GhostSkin.HorizontalScrollbar.Name;
			//_unitySkinDef.horizontalScrollbar.normal.background = ghost.GhostSkin.HorizontalScrollbar.Normal.Background;

			//_unitySkinDef.horizontalScrollbarThumb.name = ghost.GhostSkin.HorizontalScrollbarThumb.Name;
			//_unitySkinDef.horizontalScrollbarThumb.normal.background = ghost.GhostSkin.HorizontalScrollbarThumb.Normal.Background;

			//_unitySkinDef.verticalScrollbar.name = ghost.GhostSkin.VerticalScrollbar.Name;
			//_unitySkinDef.verticalScrollbar.normal.background = ghost.GhostSkin.VerticalScrollbar.Normal.Background;

			//_unitySkinDef.verticalScrollbarThumb.name = ghost.GhostSkin.VerticalScrollbarThumb.Name;
			//_unitySkinDef.verticalScrollbarThumb.normal.background = ghost.GhostSkin.VerticalScrollbarThumb.Normal.Background;

			//_unitySkinDef.horizontalSlider.name = ghost.GhostSkin.HorizontalSlider.Name;
			//_unitySkinDef.horizontalSlider.normal.background = ghost.GhostSkin.HorizontalSlider.Normal.Background;

			//_unitySkinDef.horizontalSliderThumb.name = ghost.GhostSkin.HorizontalSliderThumb.Name;
			//_unitySkinDef.horizontalSliderThumb.normal.background = ghost.GhostSkin.HorizontalSliderThumb.Normal.Background;
			//_unitySkinDef.horizontalSliderThumb.highlight.background = ghost.GhostSkin.HorizontalSliderThumb.Highlight.Background;
			//_unitySkinDef.horizontalSliderThumb.active.background = ghost.GhostSkin.HorizontalSliderThumb.Active.Background;

			//_unitySkinDef.verticalSlider.name = ghost.GhostSkin.VerticalSlider.Name;
			//_unitySkinDef.verticalSlider.normal.background = ghost.GhostSkin.VerticalSlider.Normal.Background;

			//_unitySkinDef.verticalSliderThumb.name = ghost.GhostSkin.VerticalSliderThumb.Name;
			//_unitySkinDef.verticalSliderThumb.normal.background = ghost.GhostSkin.VerticalSliderThumb.Normal.Background;
			//_unitySkinDef.verticalSliderThumb.highlight.background = ghost.GhostSkin.VerticalSliderThumb.Highlight.Background;
			//_unitySkinDef.verticalSliderThumb.active.background = ghost.GhostSkin.VerticalSliderThumb.Active.Background;

			skinLoaded = true;
		}

		private static void SkinInit (UISkinDef skin)
		{
			skin.window = new UIStyle();
			skin.box = new UIStyle();
			skin.button = new UIStyle();
			skin.toggle = new UIStyle();
			skin.textField = new UIStyle();
			skin.horizontalScrollbar = new UIStyle();
			skin.horizontalScrollbarThumb = new UIStyle();
			skin.verticalScrollbar = new UIStyle();
			skin.verticalScrollbarThumb = new UIStyle();
			skin.horizontalSlider = new UIStyle();
			skin.horizontalSliderThumb = new UIStyle();
			skin.verticalSlider = new UIStyle();
			skin.verticalSliderThumb = new UIStyle();

			skin.window.normal = new UIStyleState();
			skin.box.normal = new UIStyleState();
			skin.button.normal = new UIStyleState();
			skin.button.highlight = new UIStyleState();
			skin.button.active = new UIStyleState();
			skin.toggle.normal = new UIStyleState();
			skin.toggle.highlight = new UIStyleState();
			skin.toggle.active = new UIStyleState();
			skin.toggle.disabled = new UIStyleState();
			skin.textField.normal = new UIStyleState();
			skin.textField.highlight = new UIStyleState();
			skin.textField.active = new UIStyleState();
			skin.horizontalScrollbar.normal = new UIStyleState();
			skin.horizontalScrollbarThumb.normal = new UIStyleState();
			skin.verticalScrollbar.normal = new UIStyleState();
			skin.verticalScrollbarThumb.normal = new UIStyleState();
			skin.horizontalSlider.normal = new UIStyleState();
			skin.horizontalSliderThumb.normal = new UIStyleState();
			skin.horizontalSliderThumb.highlight = new UIStyleState();
			skin.horizontalSliderThumb.active = new UIStyleState();
			skin.verticalSlider.normal = new UIStyleState();
			skin.verticalSliderThumb.normal = new UIStyleState();
			skin.verticalSliderThumb.highlight = new UIStyleState();
			skin.verticalSliderThumb.active = new UIStyleState();
		}

		private static void processTMPPrefabs()
		{
			for (int i = loadedPrefabs.Length - 1; i >= 0; i--)
			{
				GameObject o = loadedPrefabs[i];

				if (o.name == "SCAN_MainMap")
					_mainMapPrefab = o;
				else if (o.name == "SCAN_BigMap")
					_bigMapPrefab = o;
				else if (o.name == "SCAN_Instruments")
					_instrumentsPrefab = o;
				else if (o.name == "SCAN_Overlay")
					_overlayPrefab = o;
				else if (o.name == "SCAN_Settings")
					_settingsPrefab = o;

				if (o != null)
					processTMP(o);
			}

			tmpProcessed = true;
		}

		private static void processTMP(GameObject obj)
		{
			TextHandler[] handlers = obj.GetComponentsInChildren<TextHandler>(true);

			if (handlers == null)
				return;

			for (int i = 0; i < handlers.Length; i++)
				TMProFromText(handlers[i]);
		}

		private static void TMProFromText(TextHandler handler)
		{
			if (handler == null)
				return;

			Text text = handler.GetComponent<Text>();

			if (text == null)
				return;

			string t = text.text;
			Color c = text.color;
			int i = text.fontSize;
			bool r = text.raycastTarget;
			FontStyles sty = getStyle(text.fontStyle);
			TextAlignmentOptions align = getAnchor(text.alignment);
			float spacing = text.lineSpacing;
			GameObject obj = text.gameObject;

			MonoBehaviour.DestroyImmediate(text);

			SCAN_TextMeshPro tmp = obj.AddComponent<SCAN_TextMeshPro>();

			tmp.text = t;
			tmp.color = c;
			tmp.fontSize = i;
			tmp.raycastTarget = r;
			tmp.alignment = align;
			tmp.fontStyle = sty;
			tmp.lineSpacing = spacing;

			tmp.font = Resources.Load("Fonts/Calibri SDF", typeof(TMP_FontAsset)) as TMP_FontAsset;

			//var mats = Resources.LoadAll<Material>("Fonts/Materials");

			//for (int j = mats.Length - 1; j >= 0; j--)
			//{
			//	Material mat = mats[j];

			//	SCANUtil.SCANlog("Material: {0}", mat.name);
			//}

			ScreenMessagesText smt = ScreenMessages.Instance.textPrefab;

			//SCANUtil.SCANlog("Font: {0} - Material {1} - Outline: {2:N2} - Outline Color {3:N3}"
				//, smt.text.font.name
				//, smt.text.fontSharedMaterial.name
				//, smt.text.outlineWidth
				//, smt.text.outlineColor);

			//tmp.fontSharedMaterial = smt.text.fontSharedMaterial;   //Resources.Load("Fonts/Materials/Calibri Dropshadow Outline", typeof(Material)) as Material;

			

			//tmp.font = Resources.Load("Fonts & Materials/IMPACT SDF", typeof(TMP_FontAsset)) as TMP_FontAsset;
			//tmp.fontSharedMaterial = Resources.Load("Fonts & Materials/Impact SDF - Drop Shadow", typeof(Material)) as Material;
			if (handler.Outline)
			{
				tmp.fontSharedMaterial = Resources.Load("Fonts/Materials/Calibri Dropshadow Outline", typeof(Material)) as Material;  //smt.text.fontSharedMaterial;

				if (tmp.fontMaterial)
				{

				}

				//SCANUtil.SCANlog("Material: {0} - FontMaterial is  - SharedMaterial is "
				//	//, tmp.fontSharedMaterial.name
				//, tmp.fontMaterial == null ? "Null" : "Loaded");
				//, tmp.fontSharedMaterial == null ? "Null" : "Loaded");

				tmp.outlineColor = palette.black;
				tmp.outlineWidth = handler.OutlineWidth;
			}
			else
				tmp.fontSharedMaterial = Resources.Load("Fonts/Materials/Calibri Dropshadow", typeof(Material)) as Material;

			tmp.enableWordWrapping = true;
			tmp.isOverlay = false;
			tmp.richText = true;
		}

		private static FontStyles getStyle(FontStyle style)
		{
			switch (style)
			{
				case FontStyle.Normal:
					return FontStyles.Normal;
				case FontStyle.Bold:
					return FontStyles.Bold;
				case FontStyle.Italic:
					return FontStyles.Italic;
				case FontStyle.BoldAndItalic:
					return FontStyles.Bold;
				default:
					return FontStyles.Normal;
			}
		}

		private static TextAlignmentOptions getAnchor(TextAnchor anchor)
		{
			switch (anchor)
			{
				case TextAnchor.UpperLeft:
					return TextAlignmentOptions.TopLeft;
				case TextAnchor.UpperCenter:
					return TextAlignmentOptions.Top;
				case TextAnchor.UpperRight:
					return TextAlignmentOptions.TopRight;
				case TextAnchor.MiddleLeft:
					return TextAlignmentOptions.MidlineLeft;
				case TextAnchor.MiddleCenter:
					return TextAlignmentOptions.Midline;
				case TextAnchor.MiddleRight:
					return TextAlignmentOptions.MidlineRight;
				case TextAnchor.LowerLeft:
					return TextAlignmentOptions.BottomLeft;
				case TextAnchor.LowerCenter:
					return TextAlignmentOptions.Bottom;
				case TextAnchor.LowerRight:
					return TextAlignmentOptions.BottomRight;
				default:
					return TextAlignmentOptions.Center;
			}
		}

		private static void processUIPrefabs()
		{
			for (int i = loadedPrefabs.Length - 1; i >= 0; i--)
			{
				GameObject o = loadedPrefabs[i];

				if (o != null)
					processUIComponents(o);
			}

			prefabsProcessed = true;
		}

		private static void processUIComponents(GameObject obj)
		{
			SCAN_Style[] styles = obj.GetComponentsInChildren<SCAN_Style>(true);

			if (styles == null)
				return;

			for (int i = 0; i < styles.Length; i++)
				processComponents(styles[i]);
		}

		private static void processComponents(SCAN_Style style)
		{
			if (style == null)
				return;

			UISkinDef skin = UISkinManager.defaultSkin;

			bool stock = SCAN_Settings_Config.Instance == null || SCAN_Settings_Config.Instance.StockUIStyle || _unitySkinDef == null;

			if (!stock)
				skin = _unitySkinDef;

			if (skin == null)
				return;

			switch (style.StlyeType)
			{
				case SCAN_Style.StyleTypes.Window:
					style.setImage(skin.window.normal.background);
					break;
				case SCAN_Style.StyleTypes.Box:
					style.setImage(skin.box.normal.background);
					break;
				case SCAN_Style.StyleTypes.HiddenBox:
					style.setImage(stock ? skin.box.normal.background : _clearSprite);
					break;
				case SCAN_Style.StyleTypes.Button:
					style.setButton(skin.button.normal.background, skin.button.highlight.background, skin.button.active.background, skin.button.active.background);
					break;
				case SCAN_Style.StyleTypes.HiddenButton:
					style.setButton(_clearSprite, skin.button.highlight.background, skin.button.active.background, skin.button.active.background);
					break;
				case SCAN_Style.StyleTypes.Toggle:
					if (stock)
						style.setToggle(_toggleNormal, _toggleHover, _toggleActive, _toggleActive, _toggleOn);
					else
						style.setToggle(skin.toggle.normal.background, skin.toggle.highlight.background, skin.toggle.active.background, skin.toggle.active.background, skin.toggle.disabled.background);
					break;
				case SCAN_Style.StyleTypes.ToggleButton:
					style.setToggleButton(skin.button.normal.background, skin.button.highlight.background, skin.button.active.background, skin.button.active.background);
					break;
				case SCAN_Style.StyleTypes.KSPToggle:
					style.setToggle(_toggleNormal, _toggleHover, _toggleActive, _toggleActive, _toggleOn);
					break;
				case SCAN_Style.StyleTypes.HorizontalSlider:
					style.setSlider(skin.horizontalSlider.normal.background, skin.horizontalSliderThumb.normal.background, skin.horizontalSliderThumb.highlight.background, skin.horizontalSliderThumb.active.background, skin.horizontalSliderThumb.active.background);
					break;
				default:
					break;
			}
		}
	}
}
