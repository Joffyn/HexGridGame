using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class HexUnit : MonoBehaviour
{
    #region Stats
    public string unitName;

    public float hp;
    public float atk;
    public float atkRange;

    public float initiative;
    public int movement;
    #endregion

    #region State
    public enum UnitState
    {
        Active, Inactive, Dead
    }
    public enum UnitAction
    {
        Idle, Move, Attack, RangedAttack, Earth, Water
    }

    public UnitState unitState;
    public UnitAction unitAction { get; private set; }
    #endregion

    public static HexUnit unitPrefab;
    List<HexCell> pathToTravel;
    const float travelSpeed = 4f;
    const float rotationSpeed = 180f;
    HexGameUI gameUI;
    bool isMoving;
    HexGrid grid;
  // public HexCoordinates unitCoords = new HexCoordinates();

    public GameObject highlight;

    void OnEnable()
    {
        if (location)
        {
            transform.localPosition = location.Position;
        }
        grid = GameObject.FindWithTag("HexGrid").GetComponent<HexGrid>();
    }

    private void Start()
    {
        unitAction = UnitAction.Idle;
        unitState = UnitState.Inactive;

        highlight.SetActive(false);
    }

    void Update ()
    {
        if (hp <= 0)
        {
            grid.RemoveUnit(location.Unit);
            Debug.Log(unitName + " has died");
        }


    }
    public HexCell Location
    {
        get
        {
            return location;
        }
        set
        {
            if (location)
            {
                location.Unit = null;
            }
            location = value;
            value.Unit = this;
            transform.localPosition = value.Position;
        }
    }
    HexCell location;

    public float Orientation
    {
        get
        {
            return orientation;
        }
        set
        {
            orientation = value;
            transform.localRotation = Quaternion.Euler(0f, value, 0f);
        }
    }
    float orientation;

 /*   public bool IsMoving
    {
        get
        {
            return isMoving;
        }
        set
        {
            isMoving = value;
        }
    }
*/
    #region Switch state
    public void ActivateUnit()
    {
        unitState = UnitState.Active;
        highlight.SetActive(true);
    }

    public void DeactivateUnit()
    {
        unitState = UnitState.Inactive;
        highlight.SetActive(false);
    }

    public void SetMove()
    {
        unitAction = UnitAction.Move;
    }

    public void SetIdle()
    {
        unitAction = UnitAction.Idle;
    }

    public void SetRangedAttack()
    {
        unitAction = UnitAction.RangedAttack;
    }

    public void SetAttack()
    {      
        unitAction = UnitAction.Attack;
    }

    public void SetEarthManipulation()
    {
        unitAction = UnitAction.Earth;
    }

    public void SetWaterManipulation()
    {
        unitAction = UnitAction.Water;
    }
    #endregion
    public void DoAttack(HexCell cellToBeAttacked)
    {       
        TurnToDoAction(cellToBeAttacked);
        cellToBeAttacked.Unit.hp -= unitPrefab.atk;
        Debug.Log(cellToBeAttacked.Unit.unitName + " = " + cellToBeAttacked.Unit.hp + " Hitpoints");
        Debug.Log(unitPrefab.unitName + " = " + unitPrefab.hp + " Hitpoints");
    }

    public void TurnToDoAction(HexCell cellToLookAt)
    {
        StartCoroutine(LookAt(cellToLookAt.Position));
    }

    IEnumerator LookAt(Vector3 point)
    {
        point.y = transform.localPosition.y;
        Quaternion fromRotation = transform.localRotation;
        Quaternion toRotation = Quaternion.LookRotation(point - transform.localPosition);
        float angle = Quaternion.Angle(fromRotation, toRotation);
        
        if(angle > 0f)
        {
            float speed = rotationSpeed / angle;
            for (float t = Time.deltaTime * speed; t < 1f; t += Time.deltaTime * speed)
            {
                transform.localRotation = Quaternion.Slerp(fromRotation, toRotation, t);
                yield return null;
            }
        } 

        transform.LookAt(point);
        orientation = transform.localRotation.eulerAngles.y;
    }

    public void ValidateLocation()
    {
        transform.localPosition = location.Position;
    }

    public bool IsValidDestination(HexCell cell)
    {
        return !cell.IsUnderwater && !cell.Unit;
    }

    public void Travel(List<HexCell> path)
    {
        if (path != null)
        {
            movement -= (path.Count - 1);
            Location = path[path.Count - 1];
            pathToTravel = path;
            StopAllCoroutines();
            StartCoroutine(TravelPath());
         //   Debug.Log(movement);
        }     
    } 

    IEnumerator TravelPath()
    {

        Vector3 a, b, c = pathToTravel[0].Position;
        transform.localPosition = c;
        yield return LookAt(pathToTravel[1].Position);

        float t = Time.deltaTime * travelSpeed;
        for (int i = 1; i < pathToTravel.Count; i++)
        {
            a = c;
            b = pathToTravel[i - 1].Position;
            c = (b + pathToTravel[i].Position) * 0.5f;
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
        b = pathToTravel[pathToTravel.Count - 1].Position;
        c = b;
        for (; t < 1f; t += Time.deltaTime * travelSpeed)
        {
            transform.localPosition = Bezier.GetPoint(a, b, c, t);
            Vector3 d = Bezier.GetDerivative(a, b, c, t);
            d.y = 0f;
            transform.localRotation = Quaternion.LookRotation(d);
            yield return null;
        }
        transform.localPosition = location.Position;
        orientation = transform.localRotation.eulerAngles.y;

        ListPool<HexCell>.Add(pathToTravel);
        pathToTravel = null;
       // isMoving = false;
    }

    public void Die()
    {
        location.Unit = null;
        Destroy(gameObject);
    }

    public void Save(BinaryWriter writer)
    {
        location.coordinates.Save(writer);
        writer.Write(orientation);
    }

    public static void Load(BinaryReader reader, HexGrid grid)
    {
        HexCoordinates coordinates = HexCoordinates.Load(reader);
        float orientation = reader.ReadSingle();
        grid.AddUnit(Instantiate(unitPrefab), grid.GetCell(coordinates), orientation);
    }
}
