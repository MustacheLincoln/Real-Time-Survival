using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    public float radius;
    [Range(0,360)]
    public float angle;

    public GameObject target;

    public LayerMask targetMask;
    public LayerMask obstructionMask;

    private void Start()
    {
        StartCoroutine(FOVCheck());
    }

    private IEnumerator FOVCheck()
    {
        yield return new WaitForSeconds(0.1f);
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radius, targetMask);

        if (rangeChecks.Length > 0)
        {
            Collider closest = null;
            float closestDist = Mathf.Infinity;
            foreach (Collider collider in rangeChecks)
            {
                Vector3 directionToTarget = (collider.transform.position - transform.position).normalized;

                if (Vector3.Angle(transform.forward, directionToTarget) < angle / 2)
                {
                    float distanceToTarget = Vector3.Distance(transform.position, collider.transform.position);

                    if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask))
                    {
                        float dist = Vector3.Distance(collider.transform.position, transform.position);
                        if (dist < closestDist)
                        {
                            closest = collider;
                            closestDist = dist;
                        }
                    }
                    else
                        target = null;
                }
                else
                    target = null;

            }
            if (closest)
            {
                if (target != closest.gameObject)
                {
                    target = closest.gameObject;
                }
            }
        }
        else
            target = null;

        StartCoroutine(FOVCheck());
    }
}
