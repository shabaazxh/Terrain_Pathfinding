using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{

    public Transform target;
    float speed = 10;
    Vector3[] path;
    int targetIndex;
    [SerializeField]
    GridA grid;
    RaycastHit hit;
    private void Update()
    {
        
        if(Physics.Raycast(transform.position, Vector3.forward, out hit, 1000f))
        {
            //hit.normal;
/*            if(Vector3.Angle(hit.normal, Vector3.forward) > 3)
            {
                StopAllCoroutines();
                Debug.Log("Terrain too large");
            }*/
        }
    }

    private void Start()
    {
        PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);

    }

    public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            path = newPath;
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    IEnumerator FollowPath()
    {
        Vector3 currentWayPoint = path[0];
        while (true)
        {
            if(transform.position == currentWayPoint)
            {
                targetIndex++;
                if(targetIndex >= path.Length)
                {
                    yield break;
                }
                currentWayPoint = path[targetIndex];
            }

            transform.position = Vector3.MoveTowards(transform.position, currentWayPoint, Time.deltaTime * speed);
            yield return null;
        }
    }
    public void OnDrawGizmos()
    {
/*        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.forward);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(hit.normal, transform.position); ;*/

        if (path != null)
        {
            for(int i = targetIndex; i < path.Length; i++)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(path[i], Vector3.one);

                if(i  == targetIndex)
                {
                    Gizmos.DrawLine(transform.position, path[i]);
                }
                else
                {
                    Gizmos.DrawLine(path[i - 1], path[i]);
                }
            }
        }
    }
}
