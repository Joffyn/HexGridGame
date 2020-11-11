using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class HexGrid : MonoBehaviour
{
    int chunkCountX, chunkCountZ;

    public HexCell cellPrefab;
    public HexGridChunk chunkPrefab;
    public HexUnit unitPrefab;
    public HexProjectile projectilePrefab;

    [SerializeField]
    HexGameUI specificUIOption;
    GameManager gameManager;
    
    HexCell[] cells;
    HexGridChunk[] chunks;
    public List<HexUnit> activeUnits = new List<HexUnit>();
    public List<HexUnit> inactiveUnits = new List<HexUnit>();
  //  public List<HexProjectile> activeProjectiles = new List<HexProjectile>();
  //  public List<HexProjectile> inactiveProjectiles = new List<HexProjectile>();

    public Text cellLabelPrefab;

    public Color touchedColor = Color.magenta;
    public Color[] colors;

    public Texture2D noiseSource;

    public int cellCountX = 20, cellCountZ = 15;

    public int seed;

    void Awake()
    {
        HexMetrics.noiseSource = noiseSource;
        HexMetrics.InitializeHashGrid(seed);
        HexUnit.unitPrefab = unitPrefab;
        CreateMap(cellCountX, cellCountZ);
       // specificUIOption = GameObject.FindWithTag("GameUI").GetComponent<HexGameUI>();
        gameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
    }

    void OnEnable()
    {
        if (!HexMetrics.noiseSource)
        {
            HexMetrics.noiseSource = noiseSource;
            HexMetrics.InitializeHashGrid(seed);
            HexUnit.unitPrefab = unitPrefab;
        }
    }

    public bool CreateMap(int x, int z)
    {
        if (x <= 0 || x % HexMetrics.chunkSizeX != 0 || z <= 0 || z % HexMetrics.chunkSizeZ != 0)
        {
            Debug.LogError("Unsupported map size.");
            return false;
        }
        ClearPath();
        ClearUnits();
        if (chunks != null)
        {
            for (int i = 0; i < chunks.Length; i++)
            {
                Destroy(chunks[i].gameObject);
            }
        }
        cellCountX = x;
        cellCountZ = z;
        chunkCountX = cellCountX / HexMetrics.chunkSizeX;
        chunkCountZ = cellCountZ / HexMetrics.chunkSizeZ;
        CreateChunks();
        CreateCells();
        return true;
    }

    void CreateCells()
    {
        cells = new HexCell[cellCountZ * cellCountX];

        for (int z = 0, i = 0; z < cellCountZ; z++)
        {
            for (int x = 0; x < cellCountX; x++)
            {
                CreateCell(x, z, i++);
            }
        }
    }

    void CreateChunks()
    {
        chunks = new HexGridChunk[chunkCountX * chunkCountZ];

        for (int z = 0, i = 0; z < chunkCountZ; z++)
        {
            for (int x = 0; x < chunkCountX; x++)
            {
                HexGridChunk chunk = chunks[i++] = Instantiate(chunkPrefab);
                chunk.transform.SetParent(transform);
            }
        }
    }  

    void CreateCell(int x, int z, int i)
    {
        Vector3 position;
        position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
        position.y = 0f;
        position.z = z * (HexMetrics.outerRadius * 1.5f);

        HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
        cell.transform.localPosition = position;
        cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);

        if (x > 0)
        {
            cell.SetNeighbor(HexDirection.W, cells[i - 1]);
        }
        if (z > 0)
        {
            if ((z & 1) == 0)
            {
                cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX]);
                if (x > 0)
                {
                    cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX - 1]);
                }
            }
            else
            {
                cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX]);
                if (x < cellCountX - 1)
                {
                    cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX + 1]);
                }
            }
        }

        Text label = Instantiate<Text>(cellLabelPrefab);
        label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
        cell.uiRect = label.rectTransform;
        cell.Elevation = 0;

        AddCellToChunk(x, z, cell);
    }

    void AddCellToChunk(int x, int z, HexCell cell)
    {
        int chunkX = x / HexMetrics.chunkSizeX;
        int chunkZ = z / HexMetrics.chunkSizeZ;
        HexGridChunk chunk = chunks[chunkX + chunkZ * chunkCountX];

        int localX = x - chunkX * HexMetrics.chunkSizeX;
        int localZ = z - chunkZ * HexMetrics.chunkSizeZ;
        chunk.AddCell(localX + localZ * HexMetrics.chunkSizeX, cell);
    }

    public HexCell GetCell(Vector3 position)
    {
        position = transform.InverseTransformPoint(position);
        HexCoordinates coordinates = HexCoordinates.FromPosition(position);
        int index = coordinates.X + coordinates.Z * cellCountX + coordinates.Z / 2;
        return cells[index];
    }

    public HexCell GetCell(HexCoordinates coordinates)
    {
        int z = coordinates.Z;

        if (z < 0 || z >= cellCountZ)
        {
            return null;
        }

        int x = coordinates.X + z / 2;

        if (x < 0 || x >= cellCountX)
        {
            return null;
        }

        return cells[x + z * cellCountX];
    }

    public HexCell GetCell(Ray ray)
    {
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            return GetCell(hit.point);
        }
        return null;
    }

    #region UI
    public void ShowUI(bool visible)
    {
        for (int i = 0; i < chunks.Length; i++)
        {
            chunks[i].ShowUI(visible);
        }
    }
    #endregion

    #region Distance & Pathfinding
    HexCellPriorityQueue searchFrontier;
    int searchFrontierPhase;

    HexCell currentPathFrom, currentPathTo;
    bool currentPathExists;
    bool currentPathIsReachable;

    public void FindPath(HexCell fromCell, HexCell toCell, int speed)
    {
        ClearPath();
        currentPathFrom = fromCell;
        currentPathTo = toCell;
        currentPathExists = Search(fromCell, toCell, speed);
        currentPathIsReachable = SearchReachable(toCell, speed);
        if(!gameManager.CliffMoving || !gameManager.WaterMoving)
        ShowPath(speed);
    }

    bool Search(HexCell fromCell, HexCell toCell, int speed)
    {


       // Debug.Log(specificUIOption.WaterMoving + " In grid");
        searchFrontierPhase += 2;
        if (searchFrontier == null)
        {
            searchFrontier = new HexCellPriorityQueue();
        }
        else
        {
            searchFrontier.Clear();
        }

        fromCell.SearchPhase = searchFrontierPhase;
        fromCell.Distance = 0;
        searchFrontier.Enqueue(fromCell);
        while (searchFrontier.Count > 0)
        {
            HexCell current = searchFrontier.Dequeue();
            current.SearchPhase += 1;

            if (current == toCell)
            {
                return true;
            }

            int currentTurn = (current.Distance - 1) / speed;

            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                HexCell neighbor = current.GetNeighbor(d);
               // gameUI.gameObject.AddComponent<HexGameUI>();
            
                if (neighbor == null || neighbor.SearchPhase > searchFrontierPhase)
                {
                    continue;
                }

            
                    if ((neighbor.IsUnderwater || neighbor.Unit) && !gameManager.WaterMoving)
                {
                    continue;
                }


            
                    HexEdgeType edgeType = current.GetEdgeType(neighbor);
                if (edgeType == HexEdgeType.Cliff && !gameManager.CliffMoving)
                {
                    continue;
                }

            int moveCost;

                if (current.HasRoadThroughEdge(d))
                {
                    moveCost = 1;
                }
                else if (current.Walled != neighbor.Walled)
                {
                    continue;
                }
                else
                {
                    moveCost = edgeType == HexEdgeType.Flat ? 1 : 2;
                    moveCost += neighbor.UrbanLevel + neighbor.FarmLevel + neighbor.PlantLevel;
                }

                int distance = current.Distance + moveCost;
                int turn = (distance - 1) / speed;

                if (turn == 0)
                {
                   // current.EnableValidation(Color.green);
                }

                if (turn > currentTurn)
                {
                    distance = turn * speed + moveCost;
                }

                if (neighbor.SearchPhase < searchFrontierPhase)
                {
                    neighbor.SearchPhase = searchFrontierPhase;
                    neighbor.Distance = distance;
                    neighbor.PathFrom = current;
                    neighbor.SearchHeuristic = neighbor.coordinates.DistanceTo(toCell.coordinates);
                    searchFrontier.Enqueue(neighbor);
                }
                else if (distance < neighbor.Distance)
                {
                    int oldPriority = neighbor.SearchPriority;
                    neighbor.Distance = distance;
                    neighbor.PathFrom = current;
                    searchFrontier.Change(neighbor, oldPriority);
                }
            }
        }
        return false;
    }

    public void SearchInRange(HexCell cell, int range, int action)
    {
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].Distance = int.MaxValue;
            cells[i].DisableValidation();
        }

        Queue<HexCell> frontier = new Queue<HexCell>();
        cell.Distance = 0;
        frontier.Enqueue(cell);
        while (frontier.Count > 0)
        {
            HexCell current = frontier.Dequeue();
            current.EnableValidation(Color.yellow);

            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                HexCell neighbor = current.GetNeighbor(d);
                if (neighbor == null || neighbor.Distance != int.MaxValue)
                {
                    continue;
                }
                if (current.GetEdgeType(neighbor) == HexEdgeType.Cliff && action == 0)
                {
                    continue;
                }
                if (current.Distance > (range - 1))
                {
                    break;
                }
                neighbor.Distance = current.Distance + 1;
                frontier.Enqueue(neighbor);
            }
        }
    }

    public void DisableCellValidation()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].DisableValidation();
        }
    }

    bool SearchReachable(HexCell toCell, int speed)
    {
        if (speed - toCell.Distance >= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void ShowPath(int speed)
    {
        if (currentPathExists)
        {
            HexCell current = currentPathTo;
            while (current != currentPathFrom)
            {
                int turn = (current.Distance - 1) / speed;
                current.SetLabel(turn.ToString());
                current.EnableHighlight(Color.white);
                current = current.PathFrom;
            }
        }
        currentPathFrom.EnableHighlight(Color.blue);
        currentPathTo.EnableHighlight(Color.red);
    }

    public void ClearPath()
    {
        if (currentPathExists)
        {
            HexCell current = currentPathTo;
            while (current != currentPathFrom)
            {
                current.SetLabel(null);
                current.DisableHighlight();
                current = current.PathFrom;
            }
            current.DisableHighlight();
            currentPathExists = false;
        }
        else if (currentPathFrom)
        {
            currentPathFrom.DisableHighlight();
            currentPathTo.DisableHighlight();
        }
        currentPathFrom = currentPathTo = null;
    }

    public bool HasPath
    {
        get
        {
            return currentPathExists;
        }
    }

    public bool PathIsReachable
    {
        get
        {
            return currentPathIsReachable;
        }
    }

    public List<HexCell> GetPath()
    {
        if (!currentPathExists)
        {
            return null;
        }
        List<HexCell> path = ListPool<HexCell>.Get();
        for (HexCell c = currentPathTo; c != currentPathFrom; c = c.PathFrom)
        {          
            path.Add(c);
        }
        path.Add(currentPathFrom);
        path.Reverse();
        return path;
    }

    #endregion

    #region Units
    void ClearUnits()
    {
        for (int i = 0; i < activeUnits.Count; i++)
        {
            activeUnits[i].Die();
        }
        activeUnits.Clear();
    }

    public void AddUnit(HexUnit unit, HexCell location, float orientation)
    {
        activeUnits.Add(unit);
        unit.transform.SetParent(transform, false);
        unit.Location = location;
        unit.Orientation = orientation;
    }

    public void AddProjectile(HexProjectile projectile, HexCell location, float orientation)
    {
       // activeProjectiles.Add(projectile);
        projectile.transform.SetParent(transform, false);
        projectile.Location = location;
        projectile.Orientation = orientation;
    }

/*
    public void AddEffect(HexEffect effect, HexCell location)
    {
        // activeProjectiles.Add(projectile);
        effect.transform.SetParent(transform, false);
        effect.Location = location;
    }
    */
    public void AddEffect(HexEffect effect, Vector3 deathPos)
    {
        // activeProjectiles.Add(projectile);
        effect.transform.SetParent(transform, false);
        effect.transform.position = deathPos;
    }
    public void RemoveUnit(HexUnit unit)
    {
        activeUnits.Remove(unit);
        unit.Die();
    }

 /*  public void CreateWaterTravelUnit()
    {
        HexCell cell = gameUI.TargetedWaterCells[0];
        Debug.Log("Shouldhavespawnedunit");
        Debug.Log(gameUI.TargetedWaterCells[0]);
        if (cell && !cell.Unit)
        {
            HexUnit prefab = HexUnit.unitPrefab;

            prefab.unitName = prefab.name = "Water";
            prefab.hp = 1;
            prefab.initiative = 1;
            prefab.atk = 1;
            prefab.movement = 100;

            AddUnit(Instantiate(prefab), cell, Random.Range(0f, 360f));
           // activeUnit = cell.Unit;

            HexCell currentPathTo = gameUI.TargetedWaterCells[1];
            HexCell currentPathFrom = gameUI.TargetedWaterCells[0];

            List<HexCell> path = ListPool<HexCell>.Get();
            //  List<HexCell> path = new List<HexCell>();
            for (HexCell c = currentPathTo; c != currentPathFrom; c = c.PathFrom)
            {
                path.Add(c);
            }
            path.Add(currentPathFrom);
            path.Reverse();
            cell.Unit.Travel(path);
        }
    
    }
 */
    #endregion 

    public void Save(BinaryWriter writer)
    {
        writer.Write(cellCountX);
        writer.Write(cellCountZ);

        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].Save(writer);
        }

        writer.Write(activeUnits.Count);
        for (int i = 0; i < activeUnits.Count; i++)
        {
            activeUnits[i].Save(writer);
        }
    }

    public void Load(BinaryReader reader, int header)
    {
        ClearPath();
        ClearUnits();

        int x = 20, z = 15;
        if (header >= 1)
        {
            x = reader.ReadInt32();
            z = reader.ReadInt32();
        }
        if (!CreateMap(x, z))
        {
            return;
        }
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].Load(reader);
        }
        for (int i = 0; i < chunks.Length; i++)
        {
            chunks[i].Refresh();
        }
        if (header >= 2)
        {
            int unitCount = reader.ReadInt32();
            for (int i = 0; i < unitCount; i++)
            {
                HexUnit.Load(reader, this);
            }
        }
    }
}
