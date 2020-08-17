using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GeneratorScript : MonoBehaviour {

	public Transform pc_Character;
	private static float m_fsqSize = 1f;
	private static int m_iborder = 3;


	[Range(50,300)]
	public int m_iwidth;

	[Range(40,250)]
	public int m_iheight;

	[HideInInspector]
	System.Random pc_RandomNum;

	[HideInInspector]
	int m_iFillPercent;

	[HideInInspector]
	int[,] ma_imap;

	void Start () {	//Generate initial data (seeds)
		pc_RandomNum = new System.Random ();
		m_iFillPercent = pc_RandomNum.Next (45,55);
		ma_imap = new int[width, m_iheight];
		Generatema_imap (); //Start ma_imap Generation
	}

	struct Coord {	//struct representing coordinates and also the tile corresponding to those coordinates
		public int tileX;
		public int tileY;

		public Coord(int x, int y){
			tileX=x;
			tileY=y;
		}
	}


	//helper function for getting all the tiles in the region
	//The parameters are the coordinates of the initial tile
	//A region is a set of interconnected tiles. Each tile can only belong in one region.
	List<Coord> getRegionTilesList(int startX, int startY){ 
		List<Coord> tiles = new List<Coord> ();
		int[,] ma_imapFlags = new int[width,m_iheight];
		int tileType = ma_imap [startX, startY];

		Queue<Coord> queue = new Queue<Coord> ();
		queue.Enqueue (new Coord (startX, startY));
		ma_imapFlags[startX, startY] = 1;

		Coord tile;

		while (queue.Count > 0) {
			tile = queue.Dequeue ();
			tiles.Add (tile);

			for (int i = tile.tileX - 1; i <= tile.tileX + 1; i++) {
				for (int j = tile.tileY - 1; j <= tile.tileY + 1; j++) {
					if (i >= 0 && i < m_iwidth&& j >= 0 && j < m_iheight && (i==tile.tileX ^ j==tile.tileY)) {
						if (ma_imapFlags [i, j] == 0 && ma_imap[i,j] == tileType) {
							ma_imapFlags [i, j] = 1;
							queue.Enqueue (new Coord (i, j));
						}
					}
				}
			}
		}
		return tiles;
	}

	//helper function for getting all the regions on the ma_imap.
	//the ma_imap generation process is over when the entire ma_imap is a region
	//you may select to get either all ground sets or all wall sets by passing 0 or 1 respectively to the function
	List<List<Coord>> GetAllRegions(int tiletype){
		List<List<Coord>> regions = new List<List<Coord>> ();
		int[,] ma_imapFlags = new int[width,m_iheight];

		for (int x = 0; x < width; x++) {
			for (int y = 0; y < m_iheight; y++) {
				if (ma_imapFlags [x, y] == 0 && ma_imap [x, y] == tiletype) {
					List<Coord> newRegion = getRegionTilesList (x, y);
					regions.Add (newRegion);
					foreach (Coord tile in newRegion) {
						ma_imapFlags [tile.tileX, tile.tileY] = 1;
					}
				}
			}
		}
		return regions;
	}

	//first part of the ma_imap generation process
	//randomly fill the ma_imap with empty spaces (0) and walls (1), so that is is filled with empty spaces up to about m_iFillPercent%
	private void Generatema_imap(){
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < m_iheight; y++) {
				ma_imap [x, y] = (pc_RandomNum.Next (1, 100) < m_iFillPercent) ? 1 : 0;
			}
		}

		for (int i = 0; i < 4; i++) {
			Smoothma_imap ();
		}

		Regulatema_imap ();

	}

	//part 2 of the process. Group the empty spaces and the walls together 
	//so that they form areas rather than just be noise. Repeats as needed
	void Smoothma_imap(){
		int fenceCount;
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < m_iheight; y++) {
				fenceCount = GetSurroundingTileCount (x, y);

				if (fenceCount > 4)
					ma_imap [x, y] = 1;
				else if (fenceCount < 4)
					ma_imap[x,y] = 0;
			}
		}
	}



	//Part 3 of the process. Remove very small areas caught in the middle of bigger areas so
	//that the areas are large enough to give the feeling of a cave
	void Regulatema_imap(){
		List<List<Coord>> wallRegions = GetAllRegions (1);
		List<List<Coord>> areaRegions = GetAllRegions (0);


		int regulateThreshold = 25;

		foreach (List<Coord> region in wallRegions) {
			if (region.Count < regulateThreshold) {
				foreach (Coord tile in region) {
					ma_imap [tile.tileX, tile.tileY] = 0;
				}
			}
		}

		foreach (List<Coord> region in areaRegions) {
			if (region.Count < regulateThreshold) {
				foreach (Coord tile in region) {
					ma_imap [tile.tileX, tile.tileY] = 1;
				}
			} 
		}

		if (areaRegions.Count > 1) {
			ReduceRegions (areaRegions);
		} else {
			CreateCave ();
		}

	}

	//Part 4 and final in the ma_imap generation process. Connect regions along their shortest path\
	//until there is only one region left (all areas in the ma_imap are connected). After that,
	//start generating the actual dungeon (mesh).
	void ReduceRegions(List<List<Coord>> areaRegions){

		int bestdistance = m_iheight * width;
		int distance;
		HashSet<List<Coord>> check = new HashSet<List<Coord>> ();
		Coord bestTileA = new Coord ();
		Coord bestTileB = new Coord ();
		foreach (List<Coord> roomA in areaRegions) {
			check.Add (roomA);
			foreach (List<Coord> roomB in areaRegions) {
				if (check.Contains (roomB))
					continue;

				for (int tileIndexA = 0; tileIndexA < roomA.Count; tileIndexA++) {
					for (int tileIndexB = 0; tileIndexB < roomB.Count; tileIndexB++) {
						Coord tileA = roomA [tileIndexA];
						Coord tileB = roomB [tileIndexB];
						distance = GetTileDistance (tileA, tileB);
						if (distance < bestdistance) {
							bestdistance = distance;
							bestTileA = tileA;
							bestTileB = tileB;
						}
					}
				}
			}
		}

		OpenPassageway (bestTileA, bestTileB);
		check.Clear ();
		areaRegions.Clear ();
		areaRegions = GetAllRegions (0);
		//print ("No of regions: " + areaRegions.Count);
	
		if (areaRegions.Count == 1) {
			CreateCave ();
		} else {
			ReduceRegions (areaRegions);
		}

	}

	//helper function for getting the (squared to save processing time) distance between two tiles.
	private int GetTileDistance(Coord tileA, Coord tileB){
		return Math.Abs (tileA.tileX - tileB.tileX) + Math.Abs (tileA.tileY - tileB.tileY);
	}

	//helper function for changing a wall into an empty sapce so that to clear a path.
	//Special cases for horizontal and verical passageways, plus a general form
	private void OpenPassageway(Coord tileA, Coord tileB){ // 1 point from each side to ensure a minimum passage m_iwidthof 2, maximum of 3
		if (tileA.tileX == tileB.tileX) {
			for (int i= Math.Min(tileA.tileY, tileB.tileY);i<=Math.Max(tileA.tileY, tileB.tileY); i++){
				ClearPoint (new Coord (tileA.tileX-1, i));
				ClearPoint (new Coord (tileA.tileX, i));
				ClearPoint (new Coord (tileA.tileX+1, i));
			}
		} else if (tileA.tileY == tileB.tileY) {
			for (int i= Math.Min(tileA.tileX, tileB.tileX);i<=Math.Max(tileA.tileX, tileB.tileX); i++){
				ClearPoint (new Coord (i, tileA.tileY-1));
				ClearPoint (new Coord (i, tileA.tileY));
				ClearPoint (new Coord (i, tileA.tileY+1));
			}
		} else {
			List<Coord> line = GetLine (tileA, tileB);
			foreach (Coord point in line) {
				ClearPoint (point);
			}
		}
	}

	//Get the list of wall tiles that exist in the shortest path between 2 regions
	//so that you can change them into empty spaces to form a passageway.
	List<Coord> GetLine(Coord from, Coord to){
		List<Coord> line = new List<Coord> ();

		int x = from.tileX;
		int y = from.tileY;
		int dx = to.tileX - from.tileX;
		int dy = to.tileY - from.tileY;

		int step = Math.Sign (dx);
		int gradient = Math.Sign (dy);

		int longest = Math.Abs (dx);
		int shortest = Math.Abs (dy);
		bool inverted = longest < shortest;

		if (inverted) {
			longest = shortest;
			shortest = Math.Abs (dx);
			step = gradient;
			gradient = Math.Sign (dx);
		}

		int gradAccumulation = longest / 2;

		for (int i = 0; i < longest; i++) {
			line.Add (new Coord (x, y));

			if (inverted) {
				line.Add (new Coord (x-1,y));
				line.Add (new Coord (x+1,y));
				line.Add (new Coord (x-2,y));
				line.Add (new Coord (x+2,y));
				y += step;
			} else {
				line.Add (new Coord (x,y-1));
				line.Add (new Coord (x,y+1));
				line.Add (new Coord (x,y-2));
				line.Add (new Coord (x,y+2));
				x += step;
				//line.Add (new Coord ());
			}

			gradAccumulation += shortest;
			if (gradAccumulation >= longest) {
				if (inverted)
					x += gradient;
				else
					y += gradient;
				gradAccumulation -= longest;
			}
		}

		return line;
	}

	//helper function actually changin the wall tile to empty space tile, while
	//also ensuring there are no out-of-bounds errors
	private void ClearPoint(Coord tile){
		if (tile.tileX > 0 && tile.tileY > 0 && tile.tileX < m_iwidth&& tile.tileY < m_iheight) {
			ma_imap [tile.tileX, tile.tileY] = 0;
		}
	}

	//Here the ma_imap generation process is complete and we can start generating the actual mesh.
	//Before that, the borded (wall area at the edges of the ma_imap with thickness of more than 1), if any, is removed.
	public void CreateCave(){
		int[,] fullma_imap = new int[width+border*2,m_iheight+border*2];

		for (int x = 0; x < fullma_imap.GetLength(0); x++) {
			for (int y = 0; y < fullma_imap.GetLength(1); y++) {
				if (x >= m_iborder && x < m_iwidth+ m_iborder && y >= m_iborder && y < m_iheight + border) {
					fullma_imap [x, y] = ma_imap [x - border, y - border];
				} else {
					fullma_imap [x, y] = 1;
				}
			}
		}

		MeshGenerator meshGen = GetComponent<MeshGenerator> ();
		meshGen.GenerateMesh (fullma_imap, sqSize);
		ResetCharPos ();
	}

	//helper function counting the number of walls surrouding a tile
	int GetSurroundingTileCount(int posX, int posY){
		int count=0;
		for (int neighX = posX - 1; neighX <= posX + 1; neighX++) {
			for (int neighY = posY - 1; neighY <= posY + 1; neighY++) {
				if (neighX < 0) {
					count++;
					continue;
				}
				if (neighX >= width){
					count++;
					continue;
				}
				if (neighY < 0) {
					count++;
					continue;
				}
				if (neighY >= m_iheight) {
					count++;
					continue;
				}
				if (neighX == posX && neighY == posY)
					continue;
				count += ma_imap [neighX, neighY];
			}
		}
		return count;
	}


	/* //Special debug function
	void OnDrawGizmos(){
		if (ma_imap != null) {
			for (int x = 0; x < width; x++) {
				for (int y = 0; y < m_iheight; y++) {
					Gizmos.color = (ma_imap [x, y] == 1) ? Color.black : Color.white;
					Vector3 pos = new Vector3 (-m_iwidth* 0.5f + x + 0.5f, 0, -m_iheight * 0.5f + y + 0.5f);
					Gizmos.DrawCube (pos, Vector3.one);
				}
			}
		}
	}
	*/

	float CharY=0;
	float CharX=0;

	//update function, also moving camera/pc_Character
	void Update(){

		CharY += 2*Input.GetAxis ("Mouse X");
		CharX += 2*Input.GetAxis ("Mouse Y");
		Vector3 pointTo = new Vector3 (-CharX, CharY, 0);
		pc_Character.rotation = Quaternion.Euler(pointTo);

		if (Input.GetKeyDown (KeyCode.Return)) {
			Resources.UnloadUnusedAssets ();
			m_iFillPercent = pc_RandomNum.Next (45, 55);
			ma_imap = new int[width, m_iheight];
			Generatema_imap ();
			ResetCharPos ();
		}
		if (Input.GetKey (KeyCode.S) || Input.GetKey (KeyCode.DownArrow)) {
			pc_Character.position -= 2*pc_Character.forward * Time.deltaTime;
		}
		if (Input.GetKey (KeyCode.W) || Input.GetKey (KeyCode.UpArrow)) {
			pc_Character.position += 2*pc_Character.forward * Time.deltaTime;
		}
		if (Input.GetKey (KeyCode.A) || Input.GetKey (KeyCode.LeftArrow)) {
			pc_Character.position -= 2*pc_Character.right * Time.deltaTime;
		}
		if (Input.GetKey (KeyCode.D) || Input.GetKey (KeyCode.RightArrow)) {
			pc_Character.position += 2*pc_Character.right * Time.deltaTime;
		}
	}

	//helper function to determine if a tile is on the edge of the ma_imap
	bool isEdge(int x, int y){
		x -= border;
		y -= border;

		if (ma_imap [x, y] == 1)
			return true;
		if (ma_imap [x - 1, y] == 1)
			return true;
		if (ma_imap [x + 1, y] == 1)
			return true;
		if (ma_imap [x , y - 1] == 1)
			return true;
		if (ma_imap [x , y + 1] == 1)
			return true;

		return false;
	}

	//helper function called to place the pc_Character/camera on the cave floor.
	void ResetCharPos(){
		int x, y;

		do {
			x=pc_RandomNum.Next(border+1,width+border-1);
			y=pc_RandomNum.Next(border+1,m_iheight+border-1);

		} while(isEdge (x, y));

		pc_Character.position = new Vector3 (-width/2 + (0.5f+x)*sqSize, 1, -m_iheight/2 + (0.5f+y)*sqSize);
	}
}
