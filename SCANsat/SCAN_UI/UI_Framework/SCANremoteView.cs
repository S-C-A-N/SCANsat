#region license
/*
 * [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 *
 * RemoteView - A camera looking at a GameObject, rendering to a texture.
 *
 * Copyright (c)2013 damny;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
*/
#endregion

using System;
using System.Collections.Generic;
using UnityEngine;
using palette = SCANsat.SCAN_UI.UI_Framework.SCANpalette;


namespace SCANsat.SCAN_UI.UI_Framework
{
	public class SCANremoteView {

		private static Camera cam;
		private static GameObject camgo;
		private static Shader edgeDetectShader, grayscaleShader;
		private static Material edgeDetectMaterial, grayscaleMaterial;
		private static AudioClip tick;
		private RenderTexture rt;
		/* FIXME: unused */ //private Rect camRect;
		private int updateFrame, width, height;
		private List<CrashObjectName> cons;
		private Bounds bounds;
		private int activeCon;
		private double switchTime;
		public GameObject lookat;
		public CrashObjectName lookdetail;

		public void setup ( int w , int h , GameObject focus ) {
			if (edgeDetectShader == null) {
				// simple colour based edge detection that comes with Unity Pro
				edgeDetectShader = Shader.Find ("Hidden/Edge Detect X");
				edgeDetectMaterial = new Material (edgeDetectShader);
				edgeDetectMaterial.SetFloat ("_Threshold" , 0.05f);

				// greyscale shader that comes with Unity Pro
				grayscaleShader = Shader.Find ("Hidden/Grayscale Effect");
				grayscaleMaterial = new Material (grayscaleShader);
				Texture2D t = new Texture2D (256 , 1 , TextureFormat.RGB24 , false);
				// ramp texture to render everything in dark shades of Amber,
				// except originally dark lines, which become bright Amber
				for (int i=0; i<256; ++i)
					t.SetPixel (i , 0 , palette.lerp (palette.black , palette.xkcd_Amber , i / 1024f));
				for (int i=0; i<10; ++i)
					t.SetPixel (i , 0 , palette.xkcd_Amber);
				t.Apply ();
				grayscaleMaterial.SetTexture ("_RampTex" , t);
			}
			if (lookat != focus) {
				switchTime = Time.realtimeSinceStartup;
				lookdetail = null;
				bounds = new Bounds ();
				foreach (Collider c in focus.GetComponentsInChildren<Collider>())
						bounds.Encapsulate (c.bounds);
				cons = new List<CrashObjectName> ();
				foreach (CrashObjectName con in focus.GetComponentsInChildren<CrashObjectName>()) {
					cons.Add (con);
				}
			}
			lookat = focus;
			width = w;
			height = h;
			/* FIXME: unused */ //camRect = new Rect (0 , 0 , width , height);
		}

		public void free () {
			GameObject.Destroy (camgo);
			RenderTexture.Destroy (rt);
			cam = null;
			camgo = null;
			rt = null;
		}

		public void updateCamera () {
			if (updateFrame > Time.frameCount - 5)
				return;
			if (rt == null || rt.width != width || rt.height != height) {
				rt = new RenderTexture (width , height , 32 , RenderTextureFormat.RGB565);
				rt.Create ();
			}
			if (camgo == null) {
				camgo = new GameObject ();
			}
			if (cam == null) {
				cam = camgo.AddComponent<Camera> ();
				cam.enabled = false; // so we can render on demand
				cam.targetTexture = rt;
				cam.aspect = width * 1f / height;
				cam.fieldOfView = 90;
			}

			Vector3 pos_target = lookat.transform.position;
			if (lookdetail != null)
				pos_target = lookdetail.transform.position;
			Vector3 pos_cam = FlightGlobals.ActiveVessel.transform.position;
			Vector3 target_up = (pos_target - FlightGlobals.currentMainBody.transform.position).normalized;
			Vector3 dir = (pos_target - pos_cam).normalized;
			float dist = 100; // TODO: replace with something useful for given object size
			pos_cam = pos_target - dir * dist / 2 + target_up * dist / 3;
			Vector3 cam_up = (pos_cam - FlightGlobals.currentMainBody.transform.position).normalized;

			cam.transform.position = pos_cam;
			cam.transform.LookAt (pos_target , cam_up);
			cam.farClipPlane = dist * 3;

			RenderTexture old = RenderTexture.active;
			RenderTexture.active = rt;
			cam.Render ();
			Graphics.Blit (rt , rt , edgeDetectMaterial);
			Graphics.Blit (rt , rt , grayscaleMaterial);

			RenderTexture.active = old;
			updateFrame = Time.frameCount;
		}

		private bool ticking;
		private double lastTick;
		private int tickRate = 10;

		public void drawOverlay ( Rect where , GUIStyle style , bool identified ) {
			if (tick == null) {
				tick = GameDatabase.Instance.GetAudioClip ("Squad/Sounds/sound_click_tick");
			}
			ticking = false;
			int chars = (int)((Time.realtimeSinceStartup - switchTime) * tickRate);
			Rect r = new Rect (0 , 0 , 240 , 30);
			string info = "no structures found";
			if (cons.Count == 1) {
				info = "identified 1 structure";
			} else if (cons.Count > 1) {
				info = "identified " + cons.Count.ToString () + " structures";
			}
			if (!identified)
				info = "scanning...";
			info = "> " + info;
			if (Math.Round (Time.realtimeSinceStartup) % 2 == 0)
				info += "_";
			r.x = where.x + 4;
			r.y = where.y + 8;
			GUI.Label (r , left (info , chars) , style);
			if (!identified)
				return;

			r.x = where.x + 4;
			r.y = where.y + where.height - 60;
			string sname = lookat.name;
			Vector3 lookvec = lookat.transform.position;
			if (cons.Count > 0) {
				if (Event.current.type == EventType.ScrollWheel && where.Contains (Event.current.mousePosition)) {
					activeCon += cons.Count + (Event.current.delta.y > 0 ? 1 : -1);
					Event.current.Use ();
					switchTime = Time.realtimeSinceStartup;
				}
				activeCon = activeCon % cons.Count;
				lookdetail = cons [activeCon];
				/* FIXME: unused */ //Vector3 pos = cam.WorldToScreenPoint (lookdetail.transform.position);
				lookvec = lookdetail.transform.position;
				sname = lookdetail.objectName;
			}
			if (sname != null) {
				GUI.Label (r , left (sname , chars) , style);
				r.y += 30;
				GUI.Label (r , left (distanceString ((FlightGlobals.ActiveVessel.transform.position - lookvec).magnitude) , chars) , style);
			}
			if (ticking) {
				if (Time.realtimeSinceStartup - lastTick > 1f / tickRate) {
					AudioSource.PlayClipAtPoint (tick , Camera.main.transform.position);
					lastTick = Time.realtimeSinceStartup;
				}
			}
		}

		public string left ( string s , int cnt ) {
			if (cnt >= s.Length)
				return s;
			ticking = true;
			return s.Substring (0 , cnt);
		}

		public string distanceString ( double dist ) {
			if (dist < 5000)
				return dist.ToString ("N1") + "m";
			return (dist / 1000d).ToString ("N3") + "km";
		}

		public Texture getTexture () {
			updateCamera ();
			return rt;
		}
	}
}

