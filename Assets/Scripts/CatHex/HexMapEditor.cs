using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.IO;
using UnityEngine.UI;

public class HexMapEditor : MonoBehaviour
{
    public HexGrid hexGrid;
    public Material terrainMaterial;

    #region Terrain
    int activeTerrainTypeIndex;
    int activeElevation;
    int activeWaterLevel;
    int activeUrbanLevel, activeFarmLevel, activePlantLevel;
    bool applyElevation = true;
    bool applyWaterLevel = true;
    bool applyUrbanLevel, applyFarmLevel, applyPlantLevel = true;
    int brushSize;
    OptionalToggle riverMode, roadMode, walledMode;
    #endregion

    #region Units
    public Dropdown unitDropdown;
    #endregion

    public enum EditMode
    {
        Terrain, Unit
    }
    enum OptionalToggle
    {
        Ignore, Yes, No
    }

    public EditMode editMode;
    bool applyEditMode;
    
    bool isDrag;
    HexDirection dragDirection;
    HexCell previousCell;

    void Awake()
    {
        terrainMaterial.DisableKeyword("GRID_ON");
    }

    void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (applyEditMode)
            {
                switch (editMode)
                {
                    case (EditMode.Terrain):
                        if (Input.GetMouseButton(0))
                        {
                            HandleInput();
                            return;
                        }
                        break;
                    case (EditMode.Unit):
                        if (Input.GetMouseButton(0))
                        {
                            CreateUnit();
                        }
                        if (Input.GetMouseButton(1))
                        {
                            DestroyUnit();
                        }
                        break;
                }
            }                   
        }
        previousCell = null;
    }

    void HandleInput()
    {
        HexCell currentCell = GetCellUnderCursor();
        if (currentCell)
        {
            if (previousCell && previousCell != currentCell)
            {
                ValidateDrag(currentCell);
            }
            else
            {
                isDrag = false;
            }
            EditCells(currentCell);
            previousCell = currentCell;
        }
        else
        {
            previousCell = null;
        }
    }

    HexCell GetCellUnderCursor()
    {
        return hexGrid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
    }

    void ValidateDrag(HexCell currentCell)
    {
        for (dragDirection = HexDirection.NE; dragDirection <= HexDirection.NW; dragDirection++)
        {
            if (previousCell.GetNeighbor(dragDirection) == currentCell)
            {
                isDrag = true;
                return;
            }
        }
        isDrag = false;
    }

    void EditCell(HexCell cell)
    {
        if (cell)
        {
            if (activeTerrainTypeIndex >= 0)
            {
                cell.TerrainTypeIndex = activeTerrainTypeIndex;
            }
            if (applyElevation)
            {
                cell.Elevation = activeElevation;
            }
            if (applyWaterLevel)
            {
                cell.WaterLevel = activeWaterLevel;
            }
            if (applyUrbanLevel)
            {
                cell.UrbanLevel = activeUrbanLevel;
            }
            if (applyFarmLevel)
            {
                cell.FarmLevel = activeFarmLevel;
            }
            if (applyPlantLevel)
            {
                cell.PlantLevel = activePlantLevel;
            }
            if (riverMode == OptionalToggle.No)
            {
                cell.RemoveRiver();
            }
            if (roadMode == OptionalToggle.No)
            {
                cell.RemoveRoads();
            }
            if (walledMode != OptionalToggle.Ignore)
            {
                cell.Walled = walledMode == OptionalToggle.Yes;
            }
            if (isDrag)
            {
                HexCell otherCell = cell.GetNeighbor(dragDirection.Opposite());
                if (otherCell)
                {
                    if (riverMode == OptionalToggle.Yes)
                    {
                        otherCell.SetOutgoingRiver(dragDirection);
                    }
                    if (roadMode == OptionalToggle.Yes)
                    {
                        otherCell.AddRoad(dragDirection);
                    }
                }
            }
        }
    }

    public void EarthManipulation(HexCell targetedCell)
    {
        if (targetedCell)
        {
            targetedCell.Elevation = activeElevation;
        }
    }
    void EditCells(HexCell center)
    {
        int centerX = center.coordinates.X;
        int centerZ = center.coordinates.Z;

        for (int r = 0, z = centerZ - brushSize; z <= centerZ; z++, r++)
        {
            for (int x = centerX - r; x <= centerX + brushSize; x++)
            {
                EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
            }
        }

        for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++)
        {
            for (int x = centerX - brushSize; x <= centerX + r; x++)
            {
                EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
            }
        }
    }

    public void SetTerrainTypeIndex(int index)
    {
        activeTerrainTypeIndex = index;
    }

    public void SetApplyElevation(bool toggle)
    {
        applyElevation = toggle;
    }

    public void SetElevation(float elevation)
    {
        activeElevation = (int)elevation;
    }

    public void SetApplyWaterLevel(bool toggle)
    {
        applyWaterLevel = toggle;
    }

    public void SetWaterLevel(float level)
    {
        activeWaterLevel = (int)level;
    }

    public void SetApplyUrbanLevel(bool toggle)
    {
        applyUrbanLevel = toggle;
    }

    public void SetUrbanLevel(float level)
    {
        activeUrbanLevel = (int)level;
    }

    public void SetApplyFarmLevel(bool toggle)
    {
        applyFarmLevel = toggle;
    }

    public void SetFarmLevel(float level)
    {
        activeFarmLevel = (int)level;
    }

    public void SetApplyPlantLevel(bool toggle)
    {
        applyPlantLevel = toggle;
    }

    public void SetPlantLevel(float level)
    {
        activePlantLevel = (int)level;
    }

    public void SetBrushSize(float size)
    {
        brushSize = (int)size;
    }

    public void SetRiverMode(int mode)
    {
        riverMode = (OptionalToggle)mode;
    }

    public void SetRoadMode(int mode)
    {
        roadMode = (OptionalToggle)mode;
    }

    public void SetWalledMode(int mode)
    {
        walledMode = (OptionalToggle)mode;
    }

    public void SetEditMode(int mode)
    {
        editMode = (EditMode)mode;
    }

    public void SetApplyEditMode(bool toggle)
    {
        applyEditMode = toggle;
    }

    public void ShowGrid(bool visible)
    {
        if (visible)
        {
            terrainMaterial.EnableKeyword("GRID_ON");
        }
        else
        {
            terrainMaterial.DisableKeyword("GRID_ON");
        }
    }

    #region Units
    void CreateUnit()
    {
        HexCell cell = GetCellUnderCursor();
        if (cell && !cell.Unit)
        {
            HexUnit prefab = HexUnit.unitPrefab;

            if (unitDropdown.captionText.text == "Joffy")
            {               
                prefab.unitName = prefab.name = "Joffy";
                prefab.hp = 20;
                prefab.initiative = 10;
                prefab.atk = 4;
                prefab.movement = 3;
            }
            if (unitDropdown.captionText.text == "Peppsen")
            {
                prefab.unitName = prefab.name = "Peppsen";
                prefab.hp = 12;
                prefab.initiative = 12;
                prefab.atk = 6;
                prefab.movement = 4;
            }
            if (unitDropdown.captionText.text == "Jesus")
            {
                prefab.hp = 14;
                prefab.unitName = prefab.name = "Jesus";
                prefab.initiative = 8;
                prefab.atk = 2;
                prefab.movement = 5;
            }

            hexGrid.AddUnit(Instantiate(prefab), cell, Random.Range(0f, 360f));
        }
    }

    void DestroyUnit()
    {
        HexCell cell = GetCellUnderCursor();
        if (cell && cell.Unit)
        {
            hexGrid.RemoveUnit(cell.Unit);
        }
    }
    #endregion
}
