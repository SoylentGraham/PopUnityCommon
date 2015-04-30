﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public static class GuiHelper
{
	// The texture used by DrawLine(Color)
	private static Texture2D _coloredLineTexture;
	
	// The color used by DrawLine(Color)
	private static Color _coloredLineColor;
	
	/// <summary>
	/// Draw a line between two points with the specified color and a thickness of 1
	/// </summary>
	/// <param name="lineStart">The start of the line</param>
	/// <param name="lineEnd">The end of the line</param>
	/// <param name="color">The color of the line</param>
	public static void DrawLine(Vector2 lineStart, Vector2 lineEnd, Color color)
	{
		DrawLine(lineStart, lineEnd, color, 1);
	}

	//	draw circle around point
	public static void DrawCircle(Vector2 Center, float Radius, Color color,int Segments=10)
	{
		float AngStep = 360.0f / (float)Segments;
		for (float a=0; a<=360.0f+AngStep; a+=AngStep) {
			float a_rad = Mathf.Deg2Rad * a;
			float b_rad = Mathf.Deg2Rad * (a+AngStep);
			var aOff = new Vector2( Mathf.Cos (a_rad) * Radius, Mathf.Sin (a_rad) * Radius );
			var bOff = new Vector2( Mathf.Cos (b_rad) * Radius, Mathf.Sin (b_rad) * Radius );
			DrawLine ( Center+aOff, Center+bOff, color, 1);
		}
	}

	
	/// <summary>
	/// Draw a line between two points with the specified color and thickness
	/// Inspired by code posted by Sylvan
	/// http://forum.unity3d.com/threads/17066-How-to-draw-a-GUI-2D-quot-line-quot?p=407005&viewfull=1#post407005
	/// </summary>
	/// <param name="lineStart">The start of the line</param>
	/// <param name="lineEnd">The end of the line</param>
	/// <param name="color">The color of the line</param>
	/// <param name="thickness">The thickness of the line</param>
	public static void DrawLine(Vector2 lineStart, Vector2 lineEnd, Color color, int thickness)
	{
		if (_coloredLineTexture == null || _coloredLineColor != color)
		{
			_coloredLineColor = color;
			_coloredLineTexture = new Texture2D(1, 1);
			_coloredLineTexture.name = "_coloredLineTexture";
			_coloredLineTexture.SetPixel(0, 0, _coloredLineColor);
			_coloredLineTexture.wrapMode = TextureWrapMode.Repeat;
			_coloredLineTexture.Apply();
		}
		DrawLineStretched(lineStart, lineEnd, _coloredLineTexture, thickness);
	}
	
	/// <summary>
	/// Draw a line between two points with the specified texture and thickness.
	/// The texture will be stretched to fill the drawing rectangle.
	/// Inspired by code posted by Sylvan
	/// http://forum.unity3d.com/threads/17066-How-to-draw-a-GUI-2D-quot-line-quot?p=407005&viewfull=1#post407005
	/// </summary>
	/// <param name="lineStart">The start of the line</param>
	/// <param name="lineEnd">The end of the line</param>
	/// <param name="texture">The texture of the line</param>
	/// <param name="thickness">The thickness of the line</param>
	public static void DrawLineStretched(Vector2 lineStart, Vector2 lineEnd, Texture2D texture, int thickness)
	{
		Vector2 lineVector = lineEnd - lineStart;
		//	avoid errors with nan's
		if (lineVector.sqrMagnitude < 0.0001f)
			return;

		float angle = Mathf.Rad2Deg * Mathf.Atan(lineVector.y / lineVector.x);
		if (lineVector.x < 0)
		{
			angle += 180;
		}
		
		if (thickness < 1)
		{
			thickness = 1;
		}
		
		// The center of the line will always be at the center
		// regardless of the thickness.
		int thicknessOffset = (int)Mathf.Ceil(thickness / 2);
		
		GUIUtility.RotateAroundPivot(angle,
		                             lineStart);

		GUI.DrawTexture(new Rect(lineStart.x,
		                         lineStart.y - thicknessOffset,
		                         lineVector.magnitude,
		                         thickness),
		                texture);
		                
		GUIUtility.RotateAroundPivot(-angle, lineStart);
	}
	
	/// <summary>
	/// Draw a line between two points with the specified texture and a thickness of 1
	/// The texture will be repeated to fill the drawing rectangle.
	/// </summary>
	/// <param name="lineStart">The start of the line</param>
	/// <param name="lineEnd">The end of the line</param>
	/// <param name="texture">The texture of the line</param>
	public static void DrawLine(Vector2 lineStart, Vector2 lineEnd, Texture2D texture)
	{
		DrawLine(lineStart, lineEnd, texture, 1);
	}
	
	/// <summary>
	/// Draw a line between two points with the specified texture and thickness.
	/// The texture will be repeated to fill the drawing rectangle.
	/// Inspired by code posted by Sylvan and ArenMook
	/// http://forum.unity3d.com/threads/17066-How-to-draw-a-GUI-2D-quot-line-quot?p=407005&viewfull=1#post407005
	/// http://forum.unity3d.com/threads/28247-Tile-texture-on-a-GUI?p=416986&viewfull=1#post416986
	/// </summary>
	/// <param name="lineStart">The start of the line</param>
	/// <param name="lineEnd">The end of the line</param>
	/// <param name="texture">The texture of the line</param>
	/// <param name="thickness">The thickness of the line</param>
	public static void DrawLine(Vector2 lineStart, Vector2 lineEnd, Texture2D texture, int thickness)
	{
		Vector2 lineVector = lineEnd - lineStart;
		float angle = Mathf.Rad2Deg * Mathf.Atan(lineVector.y / lineVector.x);
		if (lineVector.x < 0)
		{
			angle += 180;
		}
		
		if (thickness < 1)
		{
			thickness = 1;
		}
		
		// The center of the line will always be at the center
		// regardless of the thickness.
		int thicknessOffset = (int)Mathf.Ceil(thickness / 2);
		
		Rect drawingRect = new Rect(lineStart.x,
		                            lineStart.y - thicknessOffset,
		                            Vector2.Distance(lineStart, lineEnd),
		                            (float) thickness);
		GUIUtility.RotateAroundPivot(angle,
		                             lineStart);
		GUI.BeginGroup(drawingRect);
		{
			int drawingRectWidth = Mathf.RoundToInt(drawingRect.width);
			int drawingRectHeight = Mathf.RoundToInt(drawingRect.height);
			
			for (int y = 0; y < drawingRectHeight; y += texture.height)
			{
				for (int x = 0; x < drawingRectWidth; x += texture.width)
				{
					GUI.DrawTexture(new Rect(x,
					                         y,
					                         texture.width,
					                         texture.height),
					                texture);
				}
			}
		}
		GUI.EndGroup();
		GUIUtility.RotateAroundPivot(-angle, lineStart);
	}
}


