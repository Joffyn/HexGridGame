using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class HexProjectile : MonoBehaviour
{
    public int projectileMovement;
    public string projectileName;
    public static HexProjectile projectilePrefab;
    public List<HexCell> pathToTravel;
    const float travelSpeed = 10f;
    const float rotationSpeed = 600f;
    private Queue<IEnumerator> coroutineQueue = new Queue<IEnumerator>();

    [SerializeField]
    GameManager gameManager;


    void OnEnable()
    {
        StartCoroutine(CoroutineCoordinator());
         gameManager = GameManager.instance;
        if (location)
        {
            transform.localPosition = location.Position;
        }
    }

    void Update()
    {
       // Debug.Log(coroutineQueue.Count);
    }
    void Die()
    {
        gameManager.CellChanges();
        location.Projectile = null;
        Destroy(gameObject);
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
            value.Projectile = this;
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

    public void ValidateLocation()
    {
        transform.localPosition = location.Position;
    }

    public void Travel(List<HexCell> path)
    {
        if (path != null)
        {
            Debug.Log("Doesithappen");
            projectileMovement -= (path.Count - 1);
            Location = path[path.Count - 1];           
            pathToTravel = path;
             coroutineQueue.Enqueue(Travel(path, path[path.Count - 1]));
          // gameManager.coroutineQueue.Enqueue(Travel(path, path[path.Count - 1]));
        }
    }

    public void DestroyProjectile()
    {
        
        coroutineQueue.Enqueue(ProjectileDestroyer());
    }

    public void CellEffect(int cellEffect)
    {
            coroutineQueue.Enqueue(WaterChange());       
    }
    #region ITERATORS

    IEnumerator CoroutineCoordinator()
    {
        while (true)
        {
            while (coroutineQueue.Count > 0)
            {
                yield return StartCoroutine(coroutineQueue.Dequeue());
                Debug.Log(coroutineQueue.Count);
            }
            yield return null;
        }
    }

    IEnumerator WaterChange()
    {
        Debug.Log("Does waterchange happen?");
        gameManager.CellsToBeAffected[0].WaterLevel = 0;
        gameManager.CellsToBeAffected.Last().WaterLevel += 1;
        yield return null;
    }
    IEnumerator ProjectileDestroyer()
    {
        // gameManager = GameManager.instance;
       //  gameManager.LastCell.Projectile.Die();
        Die();
        yield return null;
    }
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
        orientation = transform.localRotation.eulerAngles.y;

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
        orientation = transform.localRotation.eulerAngles.y;
    }

    #endregion

}
