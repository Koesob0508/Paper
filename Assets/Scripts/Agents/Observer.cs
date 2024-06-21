using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Observer : MonoBehaviour
{
    public float BestRecord;
    public List<Vector2> BestTrajectory;
    public List<Vector2> LastList;
    public MaxLengthQueue<Vector2> Trajectory;
    public MaxLengthQueue<Vector2> LastTrajectory;
    public bool IsInit = false;
    public GameObject Agent;
    public GameObject Target;
    public string CurrentKey;


    void Start()
    {
        Trajectory = new MaxLengthQueue<Vector2>(5);
        LastTrajectory = new MaxLengthQueue<Vector2>(Managers.Instance.RecordLength);
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsInit) return;

        Vector3 _ = Agent.transform.position - Target.transform.position;
        Vector2 position = new Vector2(_.x, _.z);

        if (Trajectory.Count == 0)
        {
            Trajectory.Enqueue(position);
        }
        else
        {
            float distance = Vector2.Distance(position, Trajectory.GetLastElement());

            if (distance > 0.2)
            {
                Trajectory.Enqueue(position);
            }
        }

        if(LastTrajectory.Count == 0)
        {
            LastTrajectory.Enqueue(position);
        }
        else
        {
            float distance = Vector2.Distance(position, LastTrajectory.GetLastElement());

            if(distance > 0.2)
            {
                LastTrajectory.Enqueue(position);
            }
        }
    }

    public void Init(EnvData _env, GameObject _agent, GameObject _target)
    {
        IsInit = true;

        Agent = _agent;
        Target = _target;

        var _ = Agent.transform.InverseTransformPoint(Target.transform.position);
        CurrentKey = _env.GUID + "/" + (int)_.x + "/" + (int)_.z;
    }

    public void EndObserve(bool _isSuccess, float _time)
    {
        IsInit = false;

        if (!_isSuccess)
        {
            Trajectory.Clear();
            LastTrajectory.Clear();

            return;
        }

        if(Managers.Instance.OnRecord)
        {
            LastList = LastTrajectory.ToList();
            Managers.Env.UpdateLastRecord(CurrentKey, LastList);
        }

        if (BestTrajectory.Count == 0)
        {
            BestRecord = _time;
            BestTrajectory = Trajectory.ToList();

            if (Managers.Instance.OnUpdate)
            {
                Managers.Env.UpdateRecord(CurrentKey, BestRecord, BestTrajectory);
            }
        }
        else
        {
            if (BestRecord > _time)
            {
                BestRecord = _time;
                BestTrajectory = Trajectory.ToList();

                if (Managers.Instance.OnUpdate)
                {
                    Managers.Env.UpdateRecord(CurrentKey, BestRecord, BestTrajectory);
                }
            }
        }

        Trajectory.Clear();
        LastTrajectory.Clear();
    }
}
