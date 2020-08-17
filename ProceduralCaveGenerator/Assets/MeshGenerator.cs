using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour {

	public SquareGrid pc_squareGrid;
	List<Vector3> pa_v3vertices;	//list of 3D vectors
	List<int> pa_itriangles;	//list of vector indexes for triange formation

	public MeshFilter pc_walls;
	private static float km_fwallHeight = 5f;
	private static float km_fbaseHeight  = 1f;
	private int m_imapWidth;
	private int m_imapHeight;
	private float m_fsquareSize;


	Dictionary<int, List<Triangle>> pc_trigDictionary = new Dictionary<int, List<Triangle>>();
	List<List<int>> paa_ioutlines = new List<List<int>>();
	HashSet<int> pc_checkedVertices = new HashSet<int> ();
	HashSet<List<int>> pc_createdEdges = new HashSet<List<int>> ();

	//make triagles from sqares (tiles) depending on the wall shape
	//around them, indicated from the case number (the configuration of the square). 
	//This function is for the floor.
	void SquareToTrigsFloor(Square square){
		switch (square.configuration) {
		case 15:
			break;


		case 14:
			PointsToMesh (square.left, square.bottom, square.bottomLeft);
			break;
		case 13:
			PointsToMesh (square.bottomRight, square.bottom, square.right);
			break;
		case 11:
			PointsToMesh (square.topRight, square.right, square.top);
			break;
		case 7:
			PointsToMesh (square.topLeft, square.top, square.left);
			break;


		case 12:
			PointsToMesh (square.right, square.bottomRight, square.bottomLeft, square.left);
			break;
		case 9:
			PointsToMesh (square.top, square.topRight, square.bottomRight, square.bottom);
			break;
		case 6:
			PointsToMesh (square.topLeft, square.top, square.bottom, square.bottomLeft);
			break;
		case 3:
			PointsToMesh (square.topLeft, square.topRight, square.right, square.left);
			break;
		case 10:
			PointsToMesh (square.top, square.topRight, square.right, square.bottom, square.bottomLeft, square.left);
			break;
		case 5:
			PointsToMesh (square.topLeft, square.top, square.right, square.bottomRight, square.bottom, square.left);
			break;


		case 8:
			PointsToMesh (square.top, square.topRight, square.bottomRight, square.bottomLeft, square.left);
			break;
		case 4:
			PointsToMesh (square.topLeft, square.top, square.right, square.bottomRight, square.bottomLeft);
			break;
		case 2:
			PointsToMesh (square.topLeft, square.topRight, square.right, square.bottom, square.bottomLeft);
			break;
		case 1:
			PointsToMesh (square.topLeft, square.topRight, square.bottomRight, square.bottom, square.left);
			break;


		case 0:
			PointsToMesh (square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
			pc_checkedVertices.Add (square.topLeft.index);
			pc_checkedVertices.Add (square.topRight.index);
			pc_checkedVertices.Add (square.bottomLeft.index);
			pc_checkedVertices.Add (square.bottomRight.index);
			break;

		}
	}

	//make triagles from sqares (tiles) depending on the wall shape
	//around them, indicated from the case number (the configuration of the square). 
	//This function is for the ceiling.
	void SquareToTrigsCeiling(Square square){
		switch (square.configuration) {
		case 15:
			break;


		case 14:
			PointsToMeshC (square.left, square.bottom, square.bottomLeft);
			break;
		case 13:
			PointsToMeshC (square.bottomRight, square.bottom, square.right);
			break;
		case 11:
			PointsToMeshC (square.topRight, square.right, square.top);
			break;
		case 7:
			PointsToMeshC (square.topLeft, square.top, square.left);
			break;


		case 12:
			PointsToMeshC (square.right, square.bottomRight, square.bottomLeft, square.left);
			break;
		case 9:
			PointsToMeshC (square.top, square.topRight, square.bottomRight, square.bottom);
			break;
		case 6:
			PointsToMeshC (square.topLeft, square.top, square.bottom, square.bottomLeft);
			break;
		case 3:
			PointsToMeshC (square.topLeft, square.topRight, square.right, square.left);
			break;
		case 10:
			PointsToMeshC (square.top, square.topRight, square.right, square.bottom, square.bottomLeft, square.left);
			break;
		case 5:
			PointsToMeshC (square.topLeft, square.top, square.right, square.bottomRight, square.bottom, square.left);
			break;


		case 8:
			PointsToMeshC (square.top, square.topRight, square.bottomRight, square.bottomLeft, square.left);
			break;
		case 4:
			PointsToMeshC (square.topLeft, square.top, square.right, square.bottomRight, square.bottomLeft);
			break;
		case 2:
			PointsToMeshC (square.topLeft, square.topRight, square.right, square.bottom, square.bottomLeft);
			break;
		case 1:
			PointsToMeshC (square.topLeft, square.topRight, square.bottomRight, square.bottom, square.left);
			break;


		case 0:
			PointsToMeshC (square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
			pc_checkedVertices.Add (square.topLeft.index);
			pc_checkedVertices.Add (square.topRight.index);
			pc_checkedVertices.Add (square.bottomLeft.index);
			pc_checkedVertices.Add (square.bottomRight.index);
			break;

		}
	}

	//helper function for adding pa_v3vertices (3D vectors) to the list
	void AssignVertices(Node[] points){
		for (int i = 0; i < points.Length; i++) {
			if (points [i].index == -1) {
				points [i].index = pa_v3vertices.Count;
				pa_v3vertices.Add (points [i].position);
			}
		}
	}

	//helper function for selecting points to form pa_itriangles for the floor
	void PointsToMesh(params Node[] points){
		AssignVertices (points);

		if (points.Length >= 3)
			GenerateTriangle (points [0], points [1], points [2]);

		if (points.Length >= 4)
			GenerateTriangle (points [0], points [2], points [3]);

		if (points.Length >= 5)
			GenerateTriangle (points [0], points [3], points [4]);

		if (points.Length >= 6)
			GenerateTriangle (points [0], points [4], points [5]);

	}

	//helper function for selecting points to form pa_itriangles for the ceiling
	void PointsToMeshC(params Node[] points){
		AssignVertices (points);

		if (points.Length >= 3)
			GenerateTriangle (points [2], points [1], points [0]);

		if (points.Length >= 4)
			GenerateTriangle (points [3], points [2], points [0]);

		if (points.Length >= 5)
			GenerateTriangle (points [4], points [3], points [0]);

		if (points.Length >= 6)
			GenerateTriangle (points [5], points [4], points [0]);

	}

	//actual triagle generation for the mesh
	void GenerateTriangle(Node a, Node b, Node c){
		pa_itriangles.Add (a.index);
		pa_itriangles.Add (b.index);
		pa_itriangles.Add (c.index);

		Triangle triangle = new Triangle (a.index, b.index, c.index);
		AddTrigToDictionary(triangle, a.index);
		AddTrigToDictionary(triangle, b.index);
		AddTrigToDictionary(triangle, c.index);
	}

	//helper function for adding the triagle to the dictionary
	void AddTrigToDictionary(Triangle triangle, int index){
		if (pc_trigDictionary.ContainsKey (index)) {
			pc_trigDictionary [index].Add (triangle);
		} else {
			List<Triangle> trList = new List<Triangle> ();
			trList.Add (triangle);
			pc_trigDictionary.Add (index, trList);
		}
	}

	//helper function for determining if the triagle forms an outer edge
	bool isExternalEdge(int vertIndexA, int vertIndexB){
		List<Triangle> ListA = pc_trigDictionary [vertIndexA];
		int sharedTriangles = 0;
		foreach (Triangle triangle in ListA) {
			if(triangle.Contains(vertIndexB)){
				sharedTriangles++;
				if(sharedTriangles>1) break;
			}
		}
		return sharedTriangles == 1;
	}

	//helper function for getting the (index of the) other side of the edge in question
	int GetConnectedEdgeVertex(int indexA){
		List<Triangle> ListA = pc_trigDictionary [indexA];
		foreach (Triangle triangle in ListA) {
			foreach (int vertexB in triangle) {
				if (vertexB == indexA)
					continue;
				if (pc_checkedVertices.Contains (vertexB))
					continue;
				if(isExternalEdge(indexA, vertexB)){
					//print ("Edge: "+indexA + ", " + vertexB);
					return vertexB;
				}
			}
		}

		return -1;
	}

	//find (and add to variable) the pa_v3vertices that make up the outer sides of the mesh
	void CalculateMeshOutlines(){

		int currentCouple;

		for (int current = 0; current < pa_v3vertices.Count; current++) {
			if (!pc_checkedVertices.Contains (current)) {
				currentCouple = GetConnectedEdgeVertex (current);
				if (currentCouple != -1) {
					pc_checkedVertices.Add (current);

					List<int> newEdge = new List<int> ();
					newEdge.Add (current);
					paa_ioutlines.Add (newEdge);
					FollowEdge (currentCouple, paa_ioutlines.Count - 1);
					paa_ioutlines [paa_ioutlines.Count - 1].Add (current);
				}
			}
		}
	}

	//helper function for tracking pa_v3vertices along an edge to see if it is an external (outer) edge
	void FollowEdge(int vertexIndex, int edgesIndex){
		paa_ioutlines [edgesIndex].Add (vertexIndex);
		pc_checkedVertices.Add (vertexIndex);
		int nextVert = GetConnectedEdgeVertex (vertexIndex);

		if (nextVert != -1) {
			FollowEdge (nextVert, edgesIndex);
		}
	}

	//helper function for finding the "mirror" edge in the mesh. Cave floor is 
	//mirrored in the ceiling since the pc_walls are vertical.
	List<int> findCoupledEdge(List<int> edge){
		foreach (List<int> edgeC in paa_ioutlines) {
			if (edgeC == edge)
				continue;
			if (Vector3.Magnitude (pa_v3vertices [edge [0]] - pa_v3vertices [edgeC [0]]) == km_fwallHeight) {
				return edgeC;
			}
		}
		return null;
	}


	//Mesh generation function. Gets a link to the map and the tile size
	public void GenerateMesh(int[,] _map, float _size){

		Debug.Assert (km_fwallHeight % km_fbaseHeight  == 0, "km_fwallHeight must be divisible by km_fbaseHeight ");

		pc_squareGrid = new SquareGrid (_map, _size);
		pa_v3vertices = new List<Vector3>();
		pa_itriangles = new List<int>();

		m_imapWidth = _map.GetLength (0);
		m_imapHeight = _map.GetLength (1);
		m_fsquareSize = _size;

		//clear all lists (needed if reloading game)
		paa_ioutlines.Clear ();
		pc_checkedVertices.Clear ();
		pc_trigDictionary.Clear ();
		pc_createdEdges.Clear ();

		//form floor
		for (int i = 0; i < pc_squareGrid.gridF.GetLength (0); i++) {
			for (int j = 0; j < pc_squareGrid.gridF.GetLength (1); j++) {
				SquareToTrigsFloor (pc_squareGrid.gridF [i, j]);
			}
		}

		//form ceiling
		for (int i = 0; i < pc_squareGrid.gridC.GetLength (0); i++) {
			for (int j = 0; j < pc_squareGrid.gridC.GetLength (1); j++) {
				SquareToTrigsCeiling (pc_squareGrid.gridC [i, j]);
			}
		}

		//create mesh object
		Mesh caveMesh = new Mesh ();
		caveMesh.pa_v3vertices = pa_v3vertices.ToArray ();
		caveMesh.pa_itriangles = pa_itriangles.ToArray ();

		Vector2[] uvs = new Vector2[pa_v3vertices.Count];
		for (int i = 0; i < pa_v3vertices.Count; i++) {
			float pX = 10*Mathf.InverseLerp (-m_imapWidth /2* m_fsquareSize, m_imapWidth /2* m_fsquareSize, pa_v3vertices [i].x);
			float pY = 10*Mathf.InverseLerp (-m_imapHeight /2* m_fsquareSize, m_imapHeight /2* m_fsquareSize, pa_v3vertices [i].z);
			uvs [i] = new Vector2 (pX, pY);
		}
		caveMesh.uv = uvs;
		caveMesh.RecalculateNormals ();

		CreateWallComplexMesh ();

		//set mesh and collider to form
		gameObject.AddComponent<MeshFilter> ();
		MeshFilter caveMeshF = GetComponent<MeshFilter> ();
		MeshCollider col = gameObject.AddComponent<MeshCollider> ();
		col.sharedMesh = caveMesh;
		caveMeshF.mesh = caveMesh;
	}

	//form pc_walls and link them to ceiling and flooor
	//pc_walls are formed in steps, to make them more detailed and slightly more realistic
	//each wall actually includes 5 steps
	void CreateWallComplexMesh(){

		pc_walls = GetComponentInChildren<MeshFilter> ();
		pc_walls.mesh.Clear (true);
		pc_walls.sharedMesh.Clear(true);
		MeshCollider collider = pc_walls.gameObject.AddComponent<MeshCollider> ();
		collider.sharedMesh.Clear (true);


		CalculateMeshOutlines ();

		List<Vector3> wallVertices = new List<Vector3> ();
		List<int> wallTriagles = new List<int> ();
		List<Vector2> uvs = new List<Vector2> ();

		Mesh wallMesh = new Mesh ();

		int start;

		Vector3 base1, base2;
		base1 = new Vector3();
		base2=new Vector3();
		float pX, pY, diff1, diff2;
		List<int> edge = new List<int> ();
		List<int> edgeC = new List<int> ();

		int repeats = (int)(km_fwallHeight / km_fbaseHeight );
		repeats--;

		for(int a=0; a< paa_ioutlines.Count;a++) {
			edge = paa_ioutlines[a];
			edgeC = findCoupledEdge (edge);
			diff1 = 0;
			diff2 = 0;

			if (edgeC == null)
				continue;
			if (pc_createdEdges.Contains (edge) || pc_createdEdges.Contains (edgeC)) //the edges are formed in (mirrored) pairs, so if either edge has been processed before, ignore this one)
				continue;
			for (int i = 0; i < edge.Count - 1; i++) { 
				base1 = pa_v3vertices [edge [i]]; 	//left
				base2 = pa_v3vertices [edge [i + 1]]; //right
				diff1 = diff2;
				diff2 += Vector3.Magnitude(base2-base1)/(10*m_fsquareSize);
				//Debug.DrawLine(pa_v3vertices[edge[i]], pa_v3vertices[edge[i+1]], Color.red, 50000f, false);
				//Debug.DrawLine(pa_v3vertices[edgeC[i]], pa_v3vertices[edgeC[i+1]], Color.blue, 50000f, false);
				for (int k = 1; k < repeats; k++) {
					start = wallVertices.Count;

					wallVertices.Add (base1); 	//left
					pX = diff1;
					pY = Mathf.InverseLerp (pa_v3vertices [edge [i]].y, pa_v3vertices [edgeC [i]].y, base1.y);
					uvs.Add( new Vector2 (pX, pY) );

					wallVertices.Add (base2); //right
					pX = diff2;
					pY = Mathf.InverseLerp (pa_v3vertices [edge [i+1]].y, pa_v3vertices [edgeC [i+1]].y, base2.y);
					uvs.Add( new Vector2 (pX, pY) );

					base1 += (Vector3.up * km_fbaseHeight );
					base2 += (Vector3.up * km_fbaseHeight );
					//diff=base1-base2;

					wallVertices.Add (base1); 	//top left
					pX = diff1;
					pY = Mathf.InverseLerp (pa_v3vertices [edge [i]].y, pa_v3vertices [edgeC [i]].y, base1.y);
					uvs.Add( new Vector2 (pX, pY) );

					wallVertices.Add (base2); 	//top right
					pX = diff2;
					pY = Mathf.InverseLerp (pa_v3vertices [edge [i+1]].y, pa_v3vertices [edgeC [i+1]].y, base2.y);
					uvs.Add( new Vector2 (pX, pY) );

					wallTriagles.Add (start + 0);
					wallTriagles.Add (start + 2);
					wallTriagles.Add (start + 3);

					//Debug.DrawLine (wallVertices[start+0], wallVertices[start+2], Color.green, 50000f, false);
					//Debug.DrawLine (wallVertices[start+2], wallVertices[start+3], Color.green, 50000f, false);
					//Debug.DrawLine (wallVertices[start+3], wallVertices[start+0], Color.green, 50000f, false);

					wallTriagles.Add (start + 3);
					wallTriagles.Add (start + 1);
					wallTriagles.Add (start + 0);
				}

				start = wallVertices.Count;

				wallVertices.Add (base1); //left
				pX = diff1;
				pY = Mathf.InverseLerp (pa_v3vertices [edge [i]].y, pa_v3vertices [edgeC [i]].y, base2.y);
				uvs.Add( new Vector2 (pX, pY) );

				wallVertices.Add (base2); //right
				pX = diff2;
				pY = Mathf.InverseLerp (pa_v3vertices [edge [i+1]].y, pa_v3vertices [edgeC [i+1]].y, base2.y);
				uvs.Add( new Vector2 (pX, pY) );

				wallVertices.Add ( pa_v3vertices[edgeC[i]] ); 	//top left, ceiling
				pX = diff1;
				pY = 1;
				uvs.Add( new Vector2 (pX, pY) );

				wallVertices.Add ( pa_v3vertices[edgeC[i+1]] ); 	//top right, ceiling
				pX = diff2;
				pY = 1;
				uvs.Add( new Vector2 (pX, pY) );

				wallTriagles.Add (start + 0);
				wallTriagles.Add (start + 2);
				wallTriagles.Add (start + 3);

				wallTriagles.Add (start + 3);
				wallTriagles.Add (start + 1);
				wallTriagles.Add (start + 0);

				pc_createdEdges.Add (edge);
				pc_createdEdges.Add (edgeC);
				  
			}
		}



		wallMesh.pa_v3vertices = wallVertices.ToArray ();
		wallMesh.pa_itriangles = wallTriagles.ToArray ();

		wallMesh.uv = uvs.ToArray ();


		wallMesh.RecalculateNormals ();
		pc_walls.mesh = wallMesh;


		collider.sharedMesh = wallMesh;
	}

	
	////////////////////////////////////////////
	//Helper structs/classes for mesh generation and processing


	struct Triangle : IEnumerable{
		public int VertIndex1;
		public int VertIndex2;
		public int VertIndex3;

		private int[] pa_v3verticesIndex;

		public Triangle(int a, int b, int c){
			VertIndex1=a;
			VertIndex2=b;
			VertIndex3=c;

			pa_v3verticesIndex = new int[] {a,b,c};
		}

		public bool Contains(int a){
			return VertIndex1 == a || VertIndex2 == a || VertIndex3 == a;
		}

		public IEnumerator GetEnumerator(){
			return new TrigEnumerator (this);
		}

		private class TrigEnumerator : IEnumerator{
			private int pos = -1;
			private Triangle t;

			public TrigEnumerator(Triangle triangle){
				this.t = triangle;
			}

			public bool MoveNext(){
				if (pos < t.pa_v3verticesIndex.Length - 1) {
					pos++;
					return true;
				} else
					return false;
			}

			public void Reset(){
				pos = -1;
			}

			public object Current{
				get{
					return t.pa_v3verticesIndex [pos];
				}
			}
		}
	}


	public class SquareGrid {
		public Square[,] gridF; //TODO: Wherever gridF is used to make the floor, use gridC to make the ceiling
		public Square[,] gridC;

		public SquareGrid(int[,] map, float m_fsquareSize){

			//TODO: Use Parlin Noise generator in two grids to get height randomization for ceiling and floor. Max diviation to be 1.

			int XnodeCount = map.GetLength(0);
			int YnodeCount = map.GetLength(1);

			float m_imapWidth = XnodeCount*m_fsquareSize;
			float m_imapHeight = YnodeCount*m_fsquareSize;

			ControlNode[,] NodesTableF = new ControlNode[XnodeCount,YnodeCount];
			Vector3 posF;
			ControlNode[,] NodesTableC = new ControlNode[XnodeCount,YnodeCount];
			Vector3 posC;

			for(int i = 0; i<XnodeCount; i++){
				for(int j=0;j<YnodeCount;j++){
					posF = new Vector3(-m_imapWidth/2 + (i+0.5f)*m_fsquareSize, 0, -m_imapHeight/2 + (j+0.5f)*m_fsquareSize);
					NodesTableF[i,j] = new ControlNode(posF, map[i,j]==1, m_fsquareSize);
					posC = new Vector3(-m_imapWidth/2 + (i+0.5f)*m_fsquareSize, km_fwallHeight, -m_imapHeight/2 + (j+0.5f)*m_fsquareSize);
					NodesTableC[i,j] = new ControlNode(posC, map[i,j]==1, m_fsquareSize);
				}
			}

			gridF = new Square[XnodeCount-1, YnodeCount-1];
			gridC = new Square[XnodeCount-1, YnodeCount-1];

			for(int i = 0; i<XnodeCount-1; i++){
				for(int j=0;j<YnodeCount-1;j++){
					gridF[i,j] = new Square(NodesTableF[i, j], NodesTableF[i+1, j], NodesTableF[i, j+1], NodesTableF[i+1, j+1]);
					gridC[i,j] = new Square(NodesTableC[i, j], NodesTableC[i+1, j], NodesTableC[i, j+1], NodesTableC[i+1, j+1]);
				}
			}


		}
	}

	public class Square{
		public ControlNode topLeft, bottomLeft, topRight, bottomRight;
		public Node top, bottom, left, right;
		public int configuration;

		public Square(ControlNode _bL, ControlNode _bR, ControlNode _tL, ControlNode _tR){
			topLeft = _tL;
			topRight = _tR;
			bottomLeft = _bL;
			bottomRight = _bR;

			top = topLeft.right;
			bottom = bottomLeft.right;
			left = bottomLeft.above;
			right = bottomRight.above;

			configuration = 0;
			if(topLeft.active) configuration+=8;
			if(topRight.active) configuration+=4;
			if(bottomRight.active) configuration+=2;
			if(bottomLeft.active) configuration+=1;
		}
	}

	public class Node {
		public Vector3 position;
		public int index = -1;

		public Node(Vector3 _pos){
			position = _pos;
		}

	}

	public class ControlNode : Node{
		public bool active;
		public Node above, right;

		public ControlNode(Vector3 _pos, bool _act, float sqSize) : base(_pos){
			active = _act;
			above = new Node(position + Vector3.forward*sqSize/2f);
			right = new Node(position + Vector3.right*sqSize/2f);
		}
	}

}
