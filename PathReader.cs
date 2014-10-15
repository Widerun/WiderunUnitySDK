using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
public class PathReader  {

	private string fileContent;

	public List<Point> points = new List<Point>();


	public PathReader(string filename) {
		StreamReader sr = new StreamReader(filename );
		fileContent = sr.ReadToEnd();
		sr.Close();


		string[] lines = fileContent.Split(new string[] { "\r\n", "\n" }, System.StringSplitOptions.None);

		for (int l = 0; l < lines.Length; l++) {
			string[] values = lines[l].Split(new string[] { "\t", ",", ";" }, System.StringSplitOptions.None);

			if(values.Length>1) {
				string position = values[0];
				string height = values[1];

				Point p = new Point(int.Parse(position), int.Parse(height));
				points.Add(p);
			}
		}

		for (int i = 0; i < points.Count-1; i++) {
			Point s = points[i];
			Point e = points[i+1];

			int segmentLenght = e.position - s.position;
			int incrementH = e.height - s.height;

			
			float baseIncrement = Mathf.Sqrt(Mathf.Pow(segmentLenght, 2)
			                                 - Mathf.Pow(incrementH, 2));
			
			e.positionProjection = baseIncrement + s.positionProjection;
			//Debug.Log(e.positionProjection);
		}




	}


	public int GetMaxHeight() {
		int max = 0;
		for (int i = 0; i < points.Count; i++) {
			if(points[i].height>max)max=points[i].height;
		}
		return max;
	}

	public float GetMaxValue() {
		float max = 0;
		for (int i = 0; i < points.Count; i++) {
			if(points[i].positionProjection>max)max=points[i].positionProjection;
		}
		return max;
	}
	
	
	public class Point {
		public int height;
		public int position;
		public float positionProjection = 0f;
		public Point(int p, int h)
		{
			this.height = h;
			this.position = p;
		}
	}


}
