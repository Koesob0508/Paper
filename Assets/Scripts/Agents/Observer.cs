using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Observer : MonoBehaviour
{
    public List<Vector2> trajectory;
    public bool IsInit = false;
    public GameObject agent;
    public GameObject target;

    void Start()
    {
        trajectory = new List<Vector2>();
    }
    // Update is called once per frame
    void Update()
    {
        if (!IsInit) return;

        Vector3 _ = target.transform.position - agent.transform.position;
        Vector2 position = new Vector2(_.x, _.z);

        if (trajectory.Count == 0)
        {
            trajectory.Add(position);
        }
        else
        {
            float distance = Vector2.Distance(position, trajectory[trajectory.Count - 1]);

            if(distance > 0.3)
            {
                trajectory.Add(position);
            }
        }
    }

    public void Init(GameObject _agent, GameObject _target)
    {
        IsInit = true;

        agent = _agent;
        target = _target;
    }
}
