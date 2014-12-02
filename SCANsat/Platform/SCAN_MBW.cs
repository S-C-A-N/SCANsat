#region license
/* 
 *  [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 *
 * SCANsat - MonoBehaviour Window
 *  * 
 * A modified form of TriggerAu's MonoBehaviour Window class:
 * http://forum.kerbalspaceprogram.com/threads/66503-KSP-Plugin-Framework
 * 
 * Copyright (c)2013 damny;
 * Copyright (c)2014 David Grandy <david.grandy@gmail.com>;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
 *
 */
#endregion
using System;
using UnityEngine;

using Log = SCANsat.Platform.Logging.ConsoleLogger;

namespace SCANsat.Platform
{

	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
	public sealed class WindowInitialsAttribute : Attribute
	{
		public string Caption { get; set; }	// the title of the window
		public bool Visible { get; set; }	// is this window visible?

		public bool IsDragging { get; set; }	// is this window being dragged? (moved)
		public bool DragEnabled { get; set; }	// can this window be dragged?
		public bool ClampEnabled { get; set; }	// will this window be kept on screen?

		public bool TooltipsEnabled { get; set; }	// will this window show tooltips?

		public bool IsResizing { get; set; }	// is this window being resized?
		public bool ResizeEnabled { get; set; }	// can this window be resized?
		public Rect MinSize { get; set; }	// the minimum extent of the window
		public Rect MaxSize { get; set; }	// the maximum extent of the window

	}

	public abstract class SCAN_MBW : SCAN_MBE
	{

		/*
		 * MonoBehavior "LifeCycle"
		 * 
		 * [ Reset ]
		 * 		v
		 * [ Awake ]
		 * 		v
		 * [ OnEnable ]
		 * 		v>---------v
		 * 		v		[ Start ]
		 * 		v<---------<
		 * [  FixedUpdate  ]<----o
		 * 		v				 |	"Physics Loop"
		 * [ y4FixedUpdate ] ----o
		 * 		v
		 * [ Update ]
		 * 		v
		 * [ y4null ] -AND-
		 * [ y4ForSeconds ] -AND-
		 * [ y4StartLooper]
		 * 		v
		 * [ LateUpdate ]
		 * 		v
		 * [ OnWillRenderObject ]
		 * 		v
		 * 	   ...
		 * 		v
		 * [ OnPostRender ]
		 * 		v 
		 * [ OnGUI ]<-----------o	"GUI Loop"
		 * 		v				|	1. <Layout> and <Repaint>
		 * 		v				|	2. <Layout> and <Keyboard> and <Mouse>
		 * 		v---------------o
		 * [ y4waitEOF ]
		 * 		v===========> ^[ FixedUpdate ]	"Frame Loop"
		 * 		v			
		 * [ OnDisable ]
		 * 		v
		 * [ OnDestroy ]
		 * 
		 * 
		 * Some notes:
		 * 		+ These states are **individual** to each gameObject. You can not depend on
		 * 			all objects having finished [Start] just because one object is in [FixedUpdate].
		 * 		+ 
		 * 		+ The "GUI Loop" be run [2..N] times.
		 * 		+ Adapted from: http://www.richardfine.co.uk/2012/10/unity3d-monobehaviour-lifecycle/
		 * 		+ Ref: http://docs.unity3d.com/Manual/ExecutionOrder.html
		 */

		#region "Constructor"
		protected SCAN_MBW()
			: base()
		{
			//do the assembly name add so we get different windowIDs for multiple plugins
			WindowID = UnityEngine.Random.Range(1000, 2000000) + Log._AssemblyName.GetHashCode();
			_Visible = false;
			Log.Debug("WindowID:{0}", WindowID);

			//and look for any customattributes
			WindowInitialsAttribute[] attrs = (WindowInitialsAttribute[])Attribute.GetCustomAttributes(GetType(), typeof(WindowInitialsAttribute));
			foreach (WindowInitialsAttribute attr in attrs)
			{
				Visible = attr.Visible;
				WindowCaption = attr.Caption;

				IsDragging = attr.IsDragging;
				DragEnabled = attr.DragEnabled;
				ClampEnabled = attr.ClampEnabled;

				TooltipsEnabled = attr.TooltipsEnabled;

				IsResizing = attr.IsResizing;
				ResizeEnabled = attr.ResizeEnabled;

				WindowRect_Min = attr.MinSize;
				WindowRect_Max = attr.MaxSize;

			}
		}
		#endregion

		internal Int32 WindowID { get; private set; }
		internal TimeSpan DrawWindowInternalDuration { get; private set; }
		private bool _Visible;
		protected Rect WindowRect;
		protected string WindowCaption;
		protected GUIStyle WindowStyle;
		internal GUILayoutOption[] WindowOptions;

		/* dragging and clamping */

		internal Rect DragRect;
		public bool DragEnabled = false;
		public bool ClampEnabled = true;
		internal bool IsDragging = false;
		internal RectOffset ClampToScreenOffset = new RectOffset(0, 0, 0, 0);

		/* tooltips */
		protected bool TooltipShown { get; set; }
		internal Rect TooltipPosition { get { return _TooltipPosition; } }
		internal Vector2d TooltipMouseOffset = new Vector2d();
		private Rect _TooltipPosition = new Rect();

		public bool TooltipsEnabled = false;
		internal Int32 TooltipDisplayForSecs = 15;
		protected Int32 TooltipMaxWidth = 250;
		private string strToolTipText = "";
		private string strLastTooltipText = "";
		private float fltTooltipTime = 0f; // display dt for tooltip
		internal bool TooltipStatic = false; // FIXME: unused

		/* resizing windows */
		public bool ResizeEnabled;
		internal bool IsResizing = false;
		protected Rect WindowRect_Last;
		protected Rect WindowRect_Default;
		protected Rect WindowRect_Min;
		protected Rect WindowRect_Max;
		protected Rect TextureRect;
		protected Texture2D MapTexture;
		protected float dW, dH;

		internal void resetWindowPos(Rect r)
		{
			WindowRect = r;
		}

		private static float dragX, dragY;
		private static float resizeW = 0, resizeH = 0;


		public static bool inRepaint() { return (Event.current.type == EventType.Repaint); }
		public static bool inLayout() { return (Event.current.type == EventType.Layout); }

		// convenience functions to shorten the task of creating windows
		public static void growE() { GUILayout.BeginHorizontal(); }
		public static void growS() { GUILayout.BeginVertical(); }
		public static void stopE() { GUILayout.EndHorizontal(); }
		public static void stopS() { GUILayout.EndVertical(); }
		public static void growE(GUIContent c, string s) { GUILayout.BeginHorizontal(c, s); }
		public static void growS(GUIContent c, string s) { GUILayout.BeginVertical(c, s); }
		public static void growE(params GUILayoutOption[] options) { GUILayout.BeginHorizontal(options); }
		public static void growS(params GUILayoutOption[] options) { GUILayout.BeginVertical(options); }
		public static void fillS() { GUILayout.FlexibleSpace(); }
		public static void fillS(float f) { GUILayout.Space(f); }

		public static GUILayoutOption GoE() { return GUILayout.ExpandWidth(true); }
		public static GUILayoutOption GoS() { return GUILayout.ExpandHeight(true); }
		public static GUILayoutOption NoE() { return GUILayout.ExpandWidth(false); }
		public static GUILayoutOption NoS() { return GUILayout.ExpandHeight(false); }
		public static GUILayoutOption GoE(bool grow) { return GUILayout.ExpandWidth(grow); }
		public static GUILayoutOption GoS(bool grow) { return GUILayout.ExpandHeight(grow); }

		// helper functions to include tooltips
		public static GUIContent textWithTT(string label, string tooltip) { return new GUIContent(label, tooltip); }
		public static GUIContent iconWithTT(Texture tex, string tooltip) { return new GUIContent(tex, tooltip); }

		/* resizing functionality */
		public void resizeWindow()
		{
			#region top of gui_build()
			if (!inRepaint() && IsResizing)
			{

				if (Input.GetMouseButtonUp(0))
				{
					if (resizeW < WindowRect_Min.width) resizeW = WindowRect_Min.width;
					if (resizeH < WindowRect_Min.height) resizeH = WindowRect_Min.height;
					if (resizeW > WindowRect_Max.width) resizeW = WindowRect_Max.width;
					if (resizeH > WindowRect_Max.height) resizeH = WindowRect_Max.height;
					IsResizing = false;
					WindowRect_Last = new Rect(0, 0, WindowRect.width, WindowRect.height);
				}
				else
				{
					float xx = Input.mousePosition.x;
					float yy = Input.mousePosition.y;
					resizeW += xx - dragX;
					resizeH += yy - dragY;
					dragX = xx;
					dragY = yy;
					WindowRect.width = WindowRect_Last.width + resizeW;
					WindowRect.height = WindowRect_Last.height - resizeH;
				}
				if (Event.current.isMouse) Event.current.Use();
			}
			#endregion
			// ...

			#region middle of gui_build()
			dW = resizeW;
			dH = resizeH;
			if (dW < WindowRect_Min.width) dW = WindowRect_Min.width;
			if (dH < WindowRect_Min.height) dH = WindowRect_Min.height;
			dH = dW / 2f; // aspect ratio fixing

			//if (IsResizing) GUILayout.Label("", GUILayout.Width (dW), GUILayout.Height (dH));
			//else			GUILayout.Label("", GUILayout.Width(MapTexture.width), GUILayout.Height(MapTexture.height));

			//Rect maprect = GUILayoutUtility.GetLastRect ();
			//maprect.width = bigmap.mapwidth;
			//maprect.height = bigmap.mapheight;
			#endregion

			// ...

			//#region later in gui_build()
			//if (IsResizing)
			//{
			//	TextureRect.width = dW;
			//	TextureRect.height = dH;
			//	GUI.DrawTexture(TextureRect, MapTexture, ScaleMode.StretchToFill);
			//}
			//else
			//{
			//	GUI.DrawTexture(TextureRect, MapTexture);
			//}
			//#endregion
			// ...

			//float mx = Event.current.mousePosition.x - TextureRect.x;
			//float my = Event.current.mousePosition.y - TextureRect.y;

			// ...

			#region fps widget (extra)
			//Rect fpswidget = new Rect (maprect.x + maprect.width - 32 , maprect.y + maprect.height + 32 , 32 , 24);
			//GUI.Label (fpswidget , fps.Tostring ("N1"));
			#endregion

			// ...

			#region end of gui_build()
			//Rect resizer = new Rect (maprect.x + maprect.width - 24 , maprect.y + maprect.height + 8 , 24 , 24);

			Rect resizer = new Rect(WindowRect.x + WindowRect.width - 24
									, WindowRect.y + WindowRect.height - 24
									, 24, 24);

			GUI.Box(resizer, "//", SCAN_SkinsLibrary.CurrentSkin.box);

			if (Event.current.isMouse
				&& Event.current.type == EventType.MouseDown
				&& Event.current.button == 0
				&& resizer.Contains(Event.current.mousePosition))
			{
				IsResizing = true;
				WindowRect_Last = WindowRect;
				dragX = Input.mousePosition.x;
				dragY = Input.mousePosition.y;
				resizeW = TextureRect.width;
				resizeH = TextureRect.height;
				Event.current.Use();
			}
			#endregion
		}


		protected override void Awake() { Log.Debug("New MBWindow Awakened"); }
		protected virtual void DrawWindowPre(Int32 id) { }
		protected abstract void DrawWindow(Int32 id);
		protected virtual void DrawWindowPost(Int32 id) { }


		/* tooltip helper functions */
		private void SetTooltipText() { if (inRepaint()) { strToolTipText = GUI.tooltip; } }
		private bool untimed() { return (TooltipDisplayForSecs == 0); }
		private bool hasMoreTime() { return (fltTooltipTime < (float)TooltipDisplayForSecs); }
		private bool expired() { return (fltTooltipTime <= 0); }
		private bool hasChanged() { return (strToolTipText != strLastTooltipText); }
		private bool hasContent() { return (strToolTipText != ""); }
		private bool inUseNow() { return ((untimed() || hasMoreTime()) && (hasContent()) && TooltipsEnabled && Visible); }

		public bool Visible
		{
			get { return _Visible; }
			set
			{
				if (_Visible != value)
				{
					if (value)
					{
						Log.Debug("Adding Window to PostDrawQueue-{0}", WindowID); RenderingManager.AddToPostDrawQueue(5, this.DrawGUI);
					}
					else
					{
						Log.Debug("Removing Window from PostDrawQueue", WindowID); RenderingManager.RemoveFromPostDrawQueue(5, this.DrawGUI);
					}
				}
				_Visible = value;
			}
		}
		protected void DrawGUI()
		{
			string cc = "";
			GUI.skin = SCAN_SkinsLibrary.CurrentSkin;	//this sets the skin on each draw loop
			if (ClampEnabled) WindowRect = WindowRect.ClampToScreen(ClampToScreenOffset);
			if (ResizeEnabled) cc = WindowCaption + " " + WindowRect.WxH();
			else cc = WindowCaption;
			if (IsResizing) cc = WindowCaption + " " + WindowRect_Last.WxH() + " -> " + WindowRect.WxH();

			switch (WindowStyle == null)
			{
				case true: WindowRect = GUILayout.Window(WindowID, WindowRect, DrawWindowInternal, cc, WindowOptions); break;
				default: WindowRect = GUILayout.Window(WindowID, WindowRect, DrawWindowInternal, cc, WindowStyle, WindowOptions); break;
			}

			if (TooltipsEnabled) DrawToolTip();  //Draw the tooltip of its there to be drawn
			if (ResizeEnabled) resizeWindow();



		}

		private void DrawWindowInternal(Int32 id)
		{
			DateTime Duration = DateTime.Now; 	// record the start date
			DrawWindowPre(id);
			DrawWindow(id); 					// This calls the must be overridden code
			DrawWindowPost(id);
			if (TooltipsEnabled) SetTooltipText();	//Set the Tooltip variable based on whats in this window

			if (ResizeEnabled) { DragRect = new Rect(0, 0, WindowRect.width, WindowRect.height); DragRect.height -= 24; }
			else { DragRect = new Rect(0, 0, 0, 0); }

			if (DragEnabled)
				if (DragRect.height == 0 && DragRect.width == 0) GUI.DragWindow();
				else GUI.DragWindow(DragRect);
			DrawWindowInternalDuration = (DateTime.Now - Duration); //Now calc the duration
		}
		protected void DrawToolTip()
		{
			// Added drawing check to turn off tooltips when window hides
			if (inUseNow())
			{
				GUIContent contTooltip = new GUIContent(strToolTipText);
				GUIStyle styleTooltip = SCAN_SkinsLibrary.CurrentTooltip;

				// if the content of the tooltip changes then reset the counter
				if (!TooltipShown || hasChanged()) fltTooltipTime = 0f;

				// Calc the size of the Tooltip
				_TooltipPosition.x = Event.current.mousePosition.x + (float)TooltipMouseOffset.x;
				_TooltipPosition.y = Event.current.mousePosition.y + (float)TooltipMouseOffset.y;

				//do max width calc if needed
				if (TooltipMaxWidth > 0)
				{
					float minwidth, maxwidth;
					SCAN_SkinsLibrary.CurrentTooltip.CalcMinMaxWidth(contTooltip, out minwidth, out maxwidth); // figure out how wide one line would be
					_TooltipPosition.width = Math.Min(TooltipMaxWidth - SCAN_SkinsLibrary.CurrentTooltip.padding.horizontal, maxwidth); // then work out the height with a max width
					_TooltipPosition.height = SCAN_SkinsLibrary.CurrentTooltip.CalcHeight(contTooltip, TooltipPosition.width); // heres the result
				}
				else
				{ // (TooltipMaxWidth== 0)
					Vector2 Size = SCAN_SkinsLibrary.CurrentTooltip.CalcSize(contTooltip);
					_TooltipPosition.width = Size.x;
					_TooltipPosition.height = Size.y;
				}
				styleTooltip.stretchHeight = !(TooltipMaxWidth > 0);
				styleTooltip.stretchWidth = !(TooltipMaxWidth > 0);
				styleTooltip.wordWrap = (TooltipMaxWidth > 0);

				//clamp it accordingly

				if (ClampEnabled) _TooltipPosition = _TooltipPosition.ClampToScreen(ClampToScreenOffset);
				GUI.Label(TooltipPosition, contTooltip, styleTooltip);	// Draw the Tooltip
				GUI.depth = 0;											// On top of everything
				fltTooltipTime += Time.deltaTime; 	// update how long the tip has been on the screen
				TooltipShown = true;					// reset the flags
			}
			else
			{
				TooltipShown = false;
			}			// clear the flags 

			if (strToolTipText != strLastTooltipText) fltTooltipTime = 0f;
			strLastTooltipText = strToolTipText;

		}
	}
}
