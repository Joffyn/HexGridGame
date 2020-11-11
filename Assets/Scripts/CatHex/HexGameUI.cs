using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.IO;
using System.Linq;

public class HexGameUI : MonoBehaviour
{
    public HexGrid grid;
    public GameObject startButton;
    public ActiveGameUI activeGameUI;
    HexCell currentCell;
    HexUnit activeUnit;
    bool gameActive = false;
    HexCoordinates coords;
    public ElevationSlider elevationSlider;
    // [SerializeField]
    // HexProjectile projectile;

    #region Gamemanager instances

    GameManager gameManager;

    #endregion

    HexCell cellToBeManipulated;
    float targetCellElevation;
   

    #region WaterVar
    /*
     West -130 till -50, 230 till 310
     North -50 till 50, 310 till 50
     East 50 till 130, 50 till 130
     South 130 - till -130, 130 till 230
    */
    // List<HexCell> targetedWaterCells = new List<HexCell>();
    // HexCell firstCell, secondCell, thirdCell = targetedWaterCells[0];
    //  HexCell[] targetedWaterCells = new HexCell[3];
    List<HexCell> targetedCells = new List<HexCell>();

    HexCell previousCell;
    int numberOfWaterCells;
   // bool waterMoving;
    #endregion


    void Update()
    {
        activeUnit = GameManager.instance.activeUnit;
        // projectile = GameManager.instance.projectile;

        if (gameActive)
        {           
            if (!EventSystem.current.IsPointerOverGameObject() && activeUnit)
            {
                switch (activeUnit.unitAction)
                {
                    case (HexUnit.UnitAction.Idle):
                        break;
                    case (HexUnit.UnitAction.Move):
                            if (Input.GetMouseButtonDown(0))
                            {
                                DoMove();
                            }
                            else if (activeUnit.movement != 0)
                            {
                                DoPathfinding();
                            }
                            else
                            {

                            }
                        break;
                    case (HexUnit.UnitAction.Attack):
                                 UpdateCurrentCell();
                                    if (currentCell.Distance == 1) //Change this to unitrange later
                                         {
                                              if (Input.GetMouseButtonDown(0) && currentCell.Unit && currentCell.Unit != activeUnit)
                                              {
                                                activeUnit.DoAttack(currentCell);
                                              }

                                         }
                        break;
                    case (HexUnit.UnitAction.RangedAttack):
                        UpdateCurrentCell();
                        activeUnit.TurnToDoAction(currentCell);
                        activeUnit.SetDirection();
                       // Debug.Log(activeUnit.unitLookAt);
                        if (currentCell.Distance <= 3 && Input.GetMouseButtonDown(0) && currentCell.Unit != activeUnit) //this one as well
                        {
                            gameManager = GameManager.instance;
                            CellToBeManipulated = currentCell;         
                            activeUnit.TurnToDoAction(CellToBeManipulated);
                            TargetedCells.Add(activeUnit.Location);
                            TargetedCells.Add(CellToBeManipulated);
                            grid.ClearPath();
                            List<HexCell> cellsToTransfer = TargetedCells.ToList<HexCell>();
                            
                            gameManager.CreateProjectile(cellsToTransfer, 2);
                            TargetedCells.Clear();
                            
                        
                            NewButtonPressed();
                            SetIdle();
                            
                        }
                        break;

                   case (HexUnit.UnitAction.Earth):
                            UpdateCurrentCell();
                        if (currentCell.Distance <= 2 && !currentCell.IsUnderwater && Input.GetMouseButtonDown(0)) //this one as well
                        {
                            CellToBeManipulated = currentCell;
                            elevationSlider.gameObject.GetComponentInChildren<Slider>().value = cellToBeManipulated.Elevation;
                            elevationSlider.gameObject.SetActive(true);
                            elevationSlider.sliderUpdateActivate();
                            activeUnit.TurnToDoAction(CellToBeManipulated);
                            grid.ClearPath();
                            grid.SearchInRange(CellToBeManipulated, 0, 1);
                        }

                        break;

                    case (HexUnit.UnitAction.Water):
                        UpdateCurrentCell();

                        if (currentCell.IsUnderwater && currentCell.Distance <= 3 && Input.GetMouseButtonDown(0) && Input.GetKey("left shift") && numberOfWaterCells != 0)
                        {

                            CellToBeManipulated = currentCell;
                            activeUnit.TurnToDoAction(CellToBeManipulated);
                            TargetedCells.Add(CellToBeManipulated);
                            numberOfWaterCells++;
                            grid.ClearPath();
                            grid.SearchInRange(CellToBeManipulated, 0, 1);
                            List<HexCell> cellsToTransfer = TargetedCells.ToList<HexCell>();
                            gameManager.CreateProjectile(cellsToTransfer, 0);
                            TargetedCells.Clear();


                            NewButtonPressed();
                            SetIdle();
                        }

                       else if (currentCell.Distance <= 3 && Input.GetMouseButtonDown(0))
                        {
                            CellToBeManipulated = currentCell;
                          //  Debug.Log(previousCell.Position + " Previouscell " + CellToBeManipulated.Position + " CellToBeManipulated");

                            if (currentCell.IsUnderwater && numberOfWaterCells < 4 && previousCell != CellToBeManipulated)
                            {
                            gameManager = GameManager.instance;                           
                            activeUnit.TurnToDoAction(CellToBeManipulated);
                            grid.ClearPath();
                            grid.SearchInRange(CellToBeManipulated, 3, 1);
                            // TargetedWaterCells[numberOfWaterCells] = CellToBeManipulated;
                            TargetedCells.Add(CellToBeManipulated);
                            numberOfWaterCells++;
                            previousCell = currentCell;
                            }

                            if (currentCell.IsUnderwater && numberOfWaterCells == 4)
                            {
                                activeUnit.TurnToDoAction(CellToBeManipulated);
                                grid.ClearPath();
                                grid.SearchInRange(CellToBeManipulated, 0, 1);
                                List<HexCell> cellsToTransfer = TargetedCells.ToList<HexCell>();
                                gameManager.CreateProjectile(cellsToTransfer, 0);
                                TargetedCells.Clear();

                                NewButtonPressed();
                                SetIdle();
                            }

                            if (!currentCell.IsUnderwater && !currentCell.Unit && numberOfWaterCells >= 1 && TargetedCells[0].Elevation >= CellToBeManipulated.Elevation)
                            {
                                TargetedCells.Add(CellToBeManipulated);
                                numberOfWaterCells++;
                                activeUnit.TurnToDoAction(CellToBeManipulated);
                                grid.ClearPath();
                                grid.SearchInRange(CellToBeManipulated, 0, 1);
                                List<HexCell> cellsToTransfer = TargetedCells.ToList<HexCell>();
                                gameManager.CreateProjectile(cellsToTransfer, 1);
                                TargetedCells.Clear();

                                NewButtonPressed();
                                SetIdle();
                            }

                        }


                        break;
                }               
            }
            if (Input.GetKey(KeyCode.Escape))
            {
                elevationSlider.sliderUpdateDeActivate();
                elevationSlider.gameObject.SetActive(false);
                SetIdle();
            }
        }       
    }  

    public void SetEditMode(bool toggle)
    {
        this.gameObject.SetActive(!toggle);
        grid.ShowUI(!toggle);
        grid.ClearPath();
    }

    bool UpdateCurrentCell()
    {
        HexCell cell = grid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));       
        if (cell != currentCell && cell != null)
        {
            currentCell = cell;
            return true;
        }
        return false;

    }

    void DoSelection()
    {
        grid.ClearPath();
        UpdateCurrentCell();
        if (currentCell)
        {
            if (currentCell.Unit)
            {
                if (currentCell.Unit.unitState == HexUnit.UnitState.Active)
                {
                    activeUnit = currentCell.Unit;
                }
            }           
        }
    }

    void DoPathfinding()
    {
        if (UpdateCurrentCell())
        {
            if (currentCell)
            {
                grid.FindPath(activeUnit.Location, currentCell, activeUnit.movement);
            }
            else
            {
                grid.ClearPath();
            }
        }
    }

    public void SetMove()
    {
        grid.ClearPath();
        grid.SearchInRange(activeUnit.Location, activeUnit.movement, 0);
        activeUnit.SetMove();
    }

    public void SetIdle()
    {
        activeUnit.SetIdle();
        grid.ClearPath();
        grid.DisableCellValidation();
    }

    public void SetAttackButton()
    {    
        grid.ClearPath();
        grid.SearchInRange(activeUnit.Location, 1, 0);
        activeUnit.SetAttack();
    }

    public void SetRangedAttackButton()
    {
        grid.ClearPath();
        grid.SearchInRange(activeUnit.Location, 3, 1);
        activeUnit.SetRangedAttack();
    }

    public void SetEarthManipulationButton()
    {
        grid.ClearPath();
        grid.SearchInRange(activeUnit.Location, 2, 1);
        activeUnit.SetEarthManipulation();
    }

    public void SetWaterManipulationButton()
    {
        grid.ClearPath();
        grid.SearchInRange(activeUnit.Location, 3, 1);
        activeUnit.SetWaterManipulation();
    }

    public void EarthManipulationApplyButton()
    {
        TargetCellElevation = elevationSlider.ElevationSliderValue;
        CellToBeManipulated.Elevation = (int)targetCellElevation;
    }

    public HexCell CellToBeManipulated
    {
        get
        {
            return cellToBeManipulated;
        }
        set
        {
            cellToBeManipulated = value;
        }

    }

    public float TargetCellElevation
    {
        get
        {
            return targetCellElevation;
        }

        set
        {
            targetCellElevation = value;
        }
    }

    // public HexCell[] TargetedWaterCells
    // {
    //    get { return targetedWaterCells; }
    //   set { targetedWaterCells = value; }
    // }

     public List<HexCell> TargetedCells
     {
        get { return targetedCells; }
       set { targetedCells = value; }
     }
    public void NewButtonPressed()
   {
        numberOfWaterCells = 0;
        elevationSlider.gameObject.SetActive(false);
        elevationSlider.sliderUpdateDeActivate();
        //GameManager.instance.WaterMoving = false;
        GameManager.instance.CliffMoving = false;
        previousCell = null;
        grid.ClearPath();
   }

    /*    public bool WaterMoving
        {
            get
            {
                return waterMoving;
            }

            set
            {
                waterMoving = value;
            }
        }
        //  IEnumerator ElevationChange()
        //{

        //}

        void CreateWaterTravelUnit()
        {
            HexProjectile waterProjectile = GameManager.instance.projectile;
            waterProjectile.projectileMovement = 100;
            HexCell cell = TargetedWaterCells[0];
            int test = 0;

            if (cell && !cell.Unit)
            {
                grid.AddProjectile(Instantiate(waterProjectile), cell, Random.Range(0f, 360f));

                WaterMoving = true;

                for (int i = 1; i < TargetedWaterCells.Length; i++)
                {

                    grid.FindPath(TargetedWaterCells[test], TargetedWaterCells[test+1], waterProjectile.projectileMovement);
                    cell.Projectile.Travel(grid.GetPath());
                    test++;
                }

                grid.FindPath(TargetedWaterCells[0], TargetedWaterCells[1], waterProjectile.projectileMovement);
                List <HexCell> firstPath = grid.GetPath();

                grid.FindPath(TargetedWaterCells[1], TargetedWaterCells[2], waterProjectile.projectileMovement);
                List<HexCell> secondPath = grid.GetPath();
                cell.Projectile.Travel(firstPath);
                TargetedWaterCells[1].Projectile.Travel(secondPath);


            }

        }
    */

    void DoMove()
    {
        if (grid.HasPath && grid.PathIsReachable)
        {
            activeUnit.Travel(grid.GetPath());
            grid.ClearPath();
            grid.SearchInRange(activeUnit.Location, activeUnit.movement, 1);
        }
    }

    public void ActivateGameUI()
    {
        startButton.SetActive(false);
        gameActive = true;
        activeGameUI.gameObject.SetActive(true);
    }

    public void DeactivateGameUI()
    {
        startButton.SetActive(true);
        gameActive = false;
        activeGameUI.gameObject.SetActive(false);
    }
}
