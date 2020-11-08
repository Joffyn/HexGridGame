using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementManager : MonoBehaviour
{
  /*  private Queue<IEnumerator> coroutineQueue = new Queue<IEnumerator>();

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(CoroutineCoordinator());
    }


    public void Travel(List<HexCell> path)
    {
        if (path != null)
        {
            // Debug.Log("unittravels");
            projectileMovement -= (path.Count - 1);
            Location = path[path.Count - 1];
            pathToTravel = path;
            //  StopAllCoroutines();
            //  StartCoroutine(Travel());
            coroutineQueue.Enqueue(Travel(path, path[path.Count - 1]));
            // Debug.Log(movement);
        }
    }

    #region NUMERATORS

    IEnumerator CoroutineCoordinator()
    {
        while (true)
        {
            while (coroutineQueue.Count > 0)
                yield return StartCoroutine(coroutineQueue.Dequeue());
            yield return null;
        }
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
 */
}
