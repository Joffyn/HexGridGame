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
    public HexProjectile[] projectiles;
    public HexEffect effect;
    public static GameManager instance;

    BoxCollider activeUnitCollider;
    List<HexCell> cellsToBeAffected;
    bool waterMoving;
    bool cliffMoving;
    HexCell lastCell;
    int maxBounce;
    public Queue<IEnumerator> coroutineQueue = new Queue<IEnumerator>();
   // float travelSpeed = 10f;
   // float rotationSpeed = 600f;

    private void Awake()
    {
      //  StartCoroutine(CoroutineCoordinator());
        instance = this;
    }

    public void StartGame()
    {
        gameUI.ActivateGameUI();
        Debug.Log("Game Started");
        StartCoroutine(CoroutineCoordinator());
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
           // activeUnit.transform.tag = "ActiveUnit";
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
      //  activeUnit.transform.tag = "InActiveUnit";
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
     public void CreateProjectile(List<HexCell> cells, int projectileType)
     {
        coroutineQueue.Enqueue(ProjectileCreator(cells, projectileType));
     }

     IEnumerator ProjectileCreator(List<HexCell> cells, int projectileType)
    {
        // projectileType = 0 = water without cellchange
        // projectileType = 1 = water
        // projectileType = 2 = fire
        // projectileType = 10 = no bounce

        projectile.projectileMovement = 100;
       // HexCell cell = cells[0];
        int bounce = 0;

        if (cells[0])
        {
            grid.AddProjectile(Instantiate(projectiles[projectileType]), cells[0], Random.Range(0f, 360f));
            if (projectileType == 0 || projectileType == 1)
            {
               // cells[0].Projectile.gameObject.transform.GetChild(0).gameObject.SetActive(true);
                cells[0].Projectile.travelSpeed = 12f;
                WaterMoving = true;
            }
            if (projectileType == 2)
            {
              //  cells[0].Projectile.gameObject.transform.GetChild(0).gameObject.SetActive(true);
                cells[0].Projectile.travelSpeed = 18f;
                WaterMoving = true;
                CliffMoving = true;
            }

            for (int i = 1; i < cells.Count; i++)
            {
                grid.FindPath(cells[bounce], cells[bounce + 1], projectile.projectileMovement);
                //cell.Projectile.Travel(grid.GetPath());
                if (projectileType != 2)
                    Travel(cells[0].Projectile, grid.GetPath());
                else
                    coroutineQueue.Enqueue(TravelStraight(grid.GetPath(), cells[0].Projectile));
                bounce++;
            }
            grid.ClearPath();
            MaxBounce = bounce;
            CellsToBeAffected = cells;
            LastCell = cells.Last();
            if (projectileType != 2)
                coroutineQueue.Enqueue(ProjectileDestroyer(LastCell.Projectile));

            //  LastCell.Projectile.DestroyProjectile();
            yield return null;
                 

        }
    }

    public void EndOfAction(HexProjectile hexProjectile)
    {
        if(hexProjectile.ProjectileType == 1)
        {
            CellsToBeAffected[0].WaterLevel = 0;
            CellsToBeAffected.Last().WaterLevel += 1;
        }
        // grid.AddEffect(Instantiate(effect), LastCell); 
        // grid.AddEffect(Instantiate(effect), deathPos);
        // activeUnitCollider.enabled = true;
       // hexProjectile.StartProjectileDeath();
        CellsToBeAffected.Clear();
        CliffMoving = false;
        WaterMoving = false;
        grid.DisableCellValidation();
        grid.ClearPath();

        //  CellsToBeAffected[0].WaterLevel = 0;
        //   CellsToBeAffected.Last().WaterLevel += 1;
        // yield return null;

    }
    #endregion

    #region Movement

    void Travel(HexProjectile projectile, List<HexCell> path)
    {
        if (path != null)
        {
            projectile.projectileMovement -= (path.Count - 1);
            projectile.Location = path[path.Count - 1];
            projectile.pathToTravel = path;
            coroutineQueue.Enqueue(Travel(projectile, path, path[path.Count - 1]));
        }
    }

      
    IEnumerator CoroutineCoordinator()
      {
          while (true)
          {
              while (coroutineQueue.Count > 0)
              {
                  yield return StartCoroutine(coroutineQueue.Dequeue());
              }
              yield return null;
          }
      }
      
 IEnumerator Travel(HexProjectile projectile, List<HexCell> path, HexCell Location)
    {
        Vector3 a, b, c = path[0].Position;
        projectile.transform.localPosition = c;
       // yield return LookAt(projectile, path[1].Position);

        float t = Time.deltaTime * projectile.travelSpeed;
        for (int i = 1; i < path.Count; i++)
        {
            a = c;
            b = path[i - 1].Position;
            c = (b + path[i].Position) * 0.5f;
            for (; t < 1f; t += Time.deltaTime * projectile.travelSpeed)
            {
                projectile.transform.localPosition = Bezier.GetPoint(a, b, c, t);
                Vector3 d = Bezier.GetDerivative(a, b, c, t);
                d.y = 0f;
                projectile.transform.localRotation = Quaternion.LookRotation(d);
                yield return null;
            }
            t -= 1f;
        }

        a = c;
        b = path[path.Count - 1].Position;
        c = b;
        for (; t < 1f; t += Time.deltaTime * projectile.travelSpeed)
        {
            projectile.transform.localPosition = Bezier.GetPoint(a, b, c, t);
            Vector3 d = Bezier.GetDerivative(a, b, c, t);
            d.y = 0f;
            projectile.transform.localRotation = Quaternion.LookRotation(d);
            yield return null;
        }
        projectile.transform.localPosition = Location.Position;
        projectile.Orientation = projectile.transform.localRotation.eulerAngles.y;

        ListPool<HexCell>.Add(path);

        // isMoving = false;
    }


    IEnumerator LookAt(HexProjectile projectile, Vector3 point)
    {
        point.y = projectile.transform.localPosition.y;
        Quaternion fromRotation = projectile.transform.localRotation;
        Quaternion toRotation = Quaternion.LookRotation(point - projectile.transform.localPosition);
        float angle = Quaternion.Angle(fromRotation, toRotation);

        if (angle > 0f)
        {
            float speed = projectile.rotationSpeed / angle;
            for (float t = Time.deltaTime * speed; t < 1f; t += Time.deltaTime * speed)
            {
                projectile.transform.localRotation = Quaternion.Slerp(fromRotation, toRotation, t);
                yield return null;
            }
        }

        projectile.transform.LookAt(point);
        projectile.Orientation = projectile.transform.localRotation.eulerAngles.y;
    }

    #endregion

    #region Projectile

    public void StartProjectileDeath(HexProjectile projectileToDie)
    {
        coroutineQueue.Enqueue(ProjectileDestroyer(projectileToDie));
    }

    IEnumerator ProjectileDestroyer(HexProjectile projectileToDie)
    {
        // gameManager = GameManager.instance;
        //  gameManager.LastCell.Projectile.Die();

            projectileToDie.ActivateProjectileDeathEffect();
            yield return new WaitForSeconds(0.8f);

        EndOfAction(projectileToDie);
        Destroy(projectileToDie.gameObject);
        projectileToDie = null;
        yield return null;
    }

    IEnumerator TravelStraight(List<HexCell> path, HexProjectile projectile)
    {
        //activeUnitCollider = activeUnit.GetComponentInChildren<BoxCollider>();
        //activeUnitCollider.enabled = false;
        projectile.transform.localPosition = path[0].Position;
        projectile.transform.localRotation = activeUnit.transform.localRotation;
        

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        rb.velocity = LastCell.Position - path[0].Position;
        rb.velocity = rb.velocity * 3;
        CapsuleCollider projectileCollider = rb.GetComponentInChildren<CapsuleCollider>();
        projectileCollider.enabled = true;
        
        yield return null;
    }
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

    public bool CliffMoving
    {
        get
        {
            return cliffMoving;
        }

        set
        {
            cliffMoving = value;
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
