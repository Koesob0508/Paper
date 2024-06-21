using Codice.Client.Common;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.UIElements;

public class Navigation : Agent
{
    public bool On { get { return Managers.Instance.OnNavigation; } }
    public MapManager mapManager;

    public bool isFirstObserve;
    public bool isFirstAction;

    public List<float> initialObservation;
    public List<Vector2> correctTrajectory;
    private List<GameObject> currentGuides;

    public GameObject[] lastTrajectory;
    public GameObject[] answer;

    public GameObject[] navi;



    public GameObject Target
    {
        get
        {
            if (On)
            {
                if (currentGuides == null) return mapManager.Target;

                if (currentGuides.Count > 0)
                {

                    return currentGuides[0];
                }
                else
                {
                    return mapManager.Target;
                }
            }
            else
            {
                return mapManager.Target;
            }
        }
    }

    protected override void Awake()
    {
        lastTrajectory = new GameObject[Managers.Instance.RecordLength];

        answer = new GameObject[5];

        navi = new GameObject[10];

        for(int i = 0; i < Managers.Instance.RecordLength; i++)
        {
            lastTrajectory[i] = Managers.Resource.Instantiate("Prefabs/LastNavi", transform);
            lastTrajectory[i].SetActive(false);
        }

        for (int i = 0; i < 5; i++)
        {
            answer[i] = Managers.Resource.Instantiate("Prefabs/Answer", transform);
            answer[i].SetActive(false);
        }

        for (int i = 0; i < 10; i++)
        {
            navi[i] = Managers.Resource.Instantiate("Prefabs/Navi", transform);
            navi[i].SetActive(false);
        }
    }

    public override void OnEpisodeBegin()
    {
        initialObservation = new List<float>();
        correctTrajectory = new List<Vector2>();

        isFirstObserve = true;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (!gameObject.activeSelf)
        {
            initialObservation.Clear();

            for (int i = 0; i < 20; i++)
            {
                initialObservation.Add(0);
            }

            foreach (var observation in initialObservation)
            {
                sensor.AddObservation(observation);
            }

            return;
        }

        if (isFirstObserve)
        {
            initialObservation.Clear();

            Vector3 relativePosition = mapManager.Target.transform.InverseTransformPoint(mapManager.Agent.transform.position);
            // sensor.AddObservation(relativePosition);  // 상대적 위치 추가 (2개의 관찰값)
            initialObservation.Add(relativePosition.x);
            initialObservation.Add(relativePosition.z);

            foreach (var _ray in mapManager.Agent.GetComponent<FirstAgent>().CastRay())
            {
                // sensor.AddObservation(_ray / mapManager.Agent.GetComponent<FirstAgent>().rayDistance);
                initialObservation.Add(_ray / mapManager.Agent.GetComponent<FirstAgent>().rayDistance);
            }

            //
            if (Managers.Data.Trajectory.TryGetValue(mapManager.Observer.CurrentKey, out var _trajectoryData))
            {
                for (int i = 0; i < 5; i++)
                {
                    initialObservation.Add(_trajectoryData.BestTrajectoryX[i]);
                    initialObservation.Add(_trajectoryData.BestTrajectoryY[i]); // 이걸로 20개

                    correctTrajectory.Add(new Vector2(_trajectoryData.BestTrajectoryX[i], _trajectoryData.BestTrajectoryY[i]));
                }
            }
            else
            {
                for (int i = 0; i < 5; i++)
                {
                    initialObservation.Add(0f);
                    initialObservation.Add(0f); // 이걸로 20개

                    correctTrajectory.Add(Vector2.zero);
                }
            }

            isFirstObserve = false;
            isFirstAction = true;
        }

        foreach (var observation in initialObservation)
        {
            sensor.AddObservation(observation);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var action = actionsOut.ContinuousActions;

        for (int i = 0; i < 5; i++)
        {
            action[i * 2] = (10 - i) * 0.2f;
            action[i * 2 + 1] = -0.5f;
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        AddReward(-0.02f);

        var action = actions.ContinuousActions;
        if (!gameObject.activeSelf) return;

        if (isFirstAction)
        {
            // LastTrajectory 복구
            Managers.Data.LastTrajectory.TryGetValue(mapManager.Observer.CurrentKey, out var _lastTrajectory);
            
            for(int i = 0; i < _lastTrajectory.BestTrajectoryX.Count; i++)
            {
                var _ = mapManager.Target.transform.position;
                Vector3 position = new Vector3(_.x + _lastTrajectory.BestTrajectoryX[i], 1f, _.z + _lastTrajectory.BestTrajectoryY[i]);

                lastTrajectory[i].transform.position = position;

                if (_lastTrajectory.BestTrajectoryX[i] == 0 && _lastTrajectory.BestTrajectoryY[i] == 0)
                {
                    lastTrajectory[i].SetActive(false);
                    continue;
                }

                lastTrajectory[i].SetActive(true);
            }

            for (int i = 0; i < Managers.Instance.RecordLength; i++)
            {
                if (i == Managers.Instance.RecordLength - 1)
                {
                    var current = lastTrajectory[i].transform.position;
                    var next = mapManager.Target.transform.position;
                    var normal = (next - current).normalized;
                    var direction = new Vector2(normal.x, normal.z);
                    float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;

                    lastTrajectory[i].transform.rotation = Quaternion.Euler(0, angle, 0);
                }
                else
                {
                    var current = lastTrajectory[i].transform.position;
                    var next = lastTrajectory[i + 1].transform.position;
                    var normal = (next - current).normalized;
                    var direction = new Vector2(normal.x, normal.z);
                    float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;

                    lastTrajectory[i].transform.rotation = Quaternion.Euler(0, angle, 0);
                }
            }
            // correctAnswer부터 먼저 깔아보자.
            for (int i = 0; i < 5; i++)
            {
                if (correctTrajectory[i].x == 0f && correctTrajectory[i].y == 0f)
                {
                    answer[i].SetActive(false);
                    continue;
                }

                var _ = mapManager.Target.transform.position;
                Vector3 position = new Vector3(_.x + correctTrajectory[i].x, 1f, _.z + correctTrajectory[i].y);

                answer[i].transform.position = position;
            }



            for(int i = 0; i < 5; i++)
            {
                if (i == 4)
                {
                    var current = answer[i].transform.position;
                    var next = mapManager.Target.transform.position;
                    var normal = (next - current).normalized;
                    var direction = new Vector2(normal.x, normal.z);
                    float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;

                    answer[i].transform.rotation = Quaternion.Euler(0, angle, 0);
                }
                else
                {
                    var current = answer[i].transform.position;
                    var next = answer[i + 1].transform.position;
                    var normal = (next - current).normalized;
                    var direction = new Vector2(normal.x, normal.z);
                    float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;

                    answer[i].transform.rotation = Quaternion.Euler(0, angle, 0);
                }

                if (Managers.Instance.OnAnswer)
                {
                    answer[i].SetActive(true);
                }
            }

            currentGuides = new List<GameObject>();

            // action은 총 12개 (x, y) * 6
            // reward 계산은 1, 2, 3, 4, 5 번째 Vector와 비교

            if (On)
            {
                // action으로부터 경로 생성
                for (int i = 0; i < 5; i++)
                {
                    Vector3 _ = mapManager.Target.transform.position;
                    Vector3 position = new Vector3(_.x + action[i * 2], 0.25f, _.z + action[i * 2 + 1]);
                    navi[i].transform.position = position;
                    navi[i].SetActive(true);
                    currentGuides.Add(navi[i]);
                }

                for (int i = 0; i < 5; i++)
                {
                    if (i == 4)
                    {
                        var current = currentGuides[i].transform.position;
                        var next = mapManager.Target.transform.position;
                        var normal = (next - current).normalized;
                        var direction = new Vector2(normal.x, normal.z);
                        float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;

                        currentGuides[i].transform.rotation = Quaternion.Euler(0, angle, 0);
                    }
                    else
                    {
                        var current = currentGuides[i].transform.position;
                        var next = currentGuides[i + 1].transform.position;
                        var normal = (next - current).normalized;
                        var direction = new Vector2(normal.x, normal.z);
                        float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;

                        currentGuides[i].transform.rotation = Quaternion.Euler(0, angle, 0);
                    }
                }
            }
            else
            {
                foreach (var obj in navi)
                {
                    obj.SetActive(false);
                }
            }


            // reward 1 계산
            Vector2 reward1 = new Vector2(action[0], action[1]);
            float dif = Vector2.Distance(correctTrajectory[0], reward1);
            AddReward(Mathf.Pow(2, -dif) * 0.5f);

            Vector2 reward2 = new Vector2(action[2], action[3]);
            dif = Vector2.Distance(correctTrajectory[1], reward2);
            AddReward(Mathf.Pow(2, -dif) * 0.5f);

            Vector2 reward3 = new Vector2(action[4], action[5]);
            dif = Vector2.Distance(correctTrajectory[2], reward3);
            AddReward(Mathf.Pow(2, -dif) * 0.5f);

            Vector2 reward4 = new Vector2(action[6], action[7]);
            dif = Vector2.Distance(correctTrajectory[3], reward4);
            AddReward(Mathf.Pow(2, -dif) * 0.5f);

            Vector2 reward5 = new Vector2(action[8], action[9]);
            dif = Vector2.Distance(correctTrajectory[4], reward5);
            AddReward(Mathf.Pow(2, -dif) * 0.5f);

            // 그러고나서 actionbuffer 총 20개임. 20개 기반으로 지표 깔아본다. => 총 10개 깔릴 예정
            isFirstAction = false;
        }
    }

    public void OnArrival(GameObject _object)
    {
        if (currentGuides[0] != _object)
        {
            Debug.LogWarning("왜 다르지");
        }
        else
        {
            _object.SetActive(false);
            currentGuides.RemoveAt(0);
        }
    }
}