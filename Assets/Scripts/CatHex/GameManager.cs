using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public HexGrid grid;
    public HexGameUI gameUI;
    public HexUnit activeUnit;
    public HexProjectile projectile;
    public static GameManager instance;

    List<HexCell> cellsToBeAffected;
    bool waterMoving;
    HexCell lastCell;
    int maxBounce;
    public Queue<IEnumerator> coroutineQueue = new Queue<IEnumerator>();
    float travelSpeed = 10f;
    float rotationSpeed = 600f;

    private void Awake()
    {
      //  StartCoroutine(CoroutineCoordinator());
        instance = this;
    }

    public void StartGame()
    {
        gameUI.ActivateGameUI();
        Debug.Log("Game Started");

        SortByUnitInitiative();
        ActivateNextUnit();
    }

    public void EndGame()
    {
        gameUI.DeactivateGameUI();
        Debug.Log("Game Ended");
    }

    void SortByUnitInitiative()
    {
        if (grid.activeUnits.Count > 1)
        {
            grid.activeUnits.Sort((u1, u2) => u1.initiative.CompareTo(u2.initiative));
            grid.activeUnits.Reverse();
        }
    }

    void ActivateNextUnit()
    {
        if (grid.activeUnits.Count == 0)
        {
            Debug.Log("No more units to activate, end round");
            EndRound();
        }
        else
        {
            activeUnit = grid.activeUnits[0];
            activeUnit.ActivateUnit();
        }
        
    }

    public void ActivateNewProjectile()
    {
      //  projectile = grid.activeProjectiles[0];
      // Måste nu hitta hur man placerar vattnet ordentligt, samt hitta ett bättre sätt att instantiera dem för framtida tillfällen.
    }
    public void EndTurn()
    {
        grid.DisableCellValidation();
        activeUnit.DeactivateUnit();
        grid.inactiveUnits.Add(activeUnit);
        grid.activeUnits.Remove(activeUnit);
        ActivateNextUnit();
    }

    void EndRound()
    {
        grid.activeUnits.AddRange(grid.inactiveUnits);
        grid.inactiveUnits.Clear();
        SortByUnitInitiative();
        ActivateNextUnit();
    }

    #region Instantiating and world changes
     public void CreateProjectile(List<HexCell> cells, int cellEffect)
     {
        // cellEffect = 0 = nothing
        // cellEffect = 1 = water
        // cellEffect = 2 = fire
        // cellEffect = 10 = no bounce

        projectile.projectileMovement = 100;
         HexCell cell = cells[0];
         int bounce = 0;


        if (cell)
         {
            grid.AddProjectile(Instantiate(projectile), cell, Random.Range(0f, 360f));
           // cell.Projectile.gameObject.GetComponentsInChildren<>
            WaterMoving = true;

             for (int i = 1; i < cells.Count; i++)
             {
                Debug.Log("Loop goes off");
                 grid.FindPath(cells[bounce], cells[bounce + 1], projectile.projectileMovement);
                 cell.Projectile.Travel(grid.GetPath());
                 bounce++;
             }
             MaxBounce = bounce;
             CellsToBeAffected = cells;
             HexCell LastCell = cells.Last();
           // if (cellEffect == 10)
           // {
           //     CellsToBeAffected[1].Projectile.DestroyProjectile();
           // }
           // else
           // {
                LastCell.Projectile.DestroyProjectile();
          //  }

        }
     }

    public void CellChanges()
    {
        if(!CellsToBeAffected.Last().IsUnderwater && !CellsToBeAffected.Last().Unit)
        {
            Debug.Log(cellsToBeAffected.Count);
            CellsToBeAffected[0].WaterLevel = 0;
            CellsToBeAffected.Last().WaterLevel += 1;
        }
        CellsToBeAffected.Clear();

        //  CellsToBeAffected[0].WaterLevel = 0;
        //   CellsToBeAffected.Last().WaterLevel += 1;
        // yield return null;

    }
    #endregion

    #region Movement

    /*  void Travel(List<HexCell> path)
      {
          if (path != null)
          {
              projectile.projectileMovement -= (path.Count - 1);
              projectile.Location = path[path.Count - 1];
              projectile.pathToTravel = path;
              coroutineQueue.Enqueue(Travel(path, path[path.Count - 1]));
          }
      }

            public void CellChanges()
    {
        CellsToBeAffected[0].WaterLevel = 0;
        CellsToBeAffected.Last().WaterLevel += 1;
    }
      
    IEnumerator CoroutineCoordinator()
      {
          while (true)
          {
              while (coroutineQueue.Count > 0)
              {
                  Debug.Log(coroutineQueue.Count);
                  yield return StartCoroutine(coroutineQueue.Dequeue());
              }
              yield return null;
          }
      }
      /*
      IEnumerator Travel(List<HexCell> path, HexCell Location)
      {
          Vector3 a, b, c = path[0].Position;
          transform.localPosition = c;
          yield return LookAt(path[1].Position);

          float t = Time.deltaTime * travelSpeed;
          for (int i = 1; i < path.Count; i++)
          {
              a = c;
              b = path[i - 1].Position;
              c = (b + path[i].Position) * 0.5f;
              for (; t < 1f; t += Time.deltaTime * travelSpeed)
              {
                  transform.localPosition = Bezier.GetPoint(a, b, c, t);
                  Vector3 d = Bezier.GetDerivative(a, b, c, t);
                  d.y = 0f;
                  transform.localRotation = Quaternion.LookRotation(d);
                  yield return null;
              }
              t -= 1f;
          }

          a = c;
          b = path[path.Count - 1].Position;
          c = b;
          for (; t < 1f; t += Time.deltaTime * travelSpeed)
          {
              transform.localPosition = Bezier.GetPoint(a, b, c, t);
              Vector3 d = Bezier.GetDerivative(a, b, c, t);
              d.y = 0f;
              transform.localRotation = Quaternion.LookRotation(d);
              yield return null;
          }
          transform.localPosition = Location.Position;
          projectile.Orientation = transform.localRotation.eulerAngles.y;

          ListPool<HexCell>.Add(path);
          path = null;

          // isMoving = false;
      }



      IEnumerator LookAt(Vector3 point)
      {
          point.y = transform.localPosition.y;
          Quaternion fromRotation = transform.localRotation;
          Quaternion toRotation = Quaternion.LookRotation(point - transform.localPosition);
          float angle = Quaternion.Angle(fromRotation, toRotation);

          if (angle > 0f)
          {
              float speed = rotationSpeed / angle;
              for (float t = Time.deltaTime * speed; t < 1f; t += Time.deltaTime * speed)
              {
                  transform.localRotation = Quaternion.Slerp(fromRotation, toRotation, t);
                  yield return null;
              }
          }

          transform.LookAt(point);
          projectile.Orientation = transform.localRotation.eulerAngles.y;
      }
      */
    #endregion

    #region Getters n setters
    public bool WaterMoving
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

    public HexCell LastCell
    {
        get
        {
            return lastCell;
        }

        set
        {
            lastCell = value;
        }
    }

    public int MaxBounce
    {
        get
        {
            return maxBounce;
        }

        set
        {
            maxBounce = value;
        }
    }
    
    public List<HexCell> CellsToBeAffected
    {
        get
        {
            return cellsToBeAffected;
        }

        set
        {
            cellsToBeAffected = value;
        }
    }
    #endregion
}
