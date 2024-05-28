using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class SuperAgent : Agent
{
    public Rigidbody carRigidbody;
    public GameObject goal;
    public CarController carController;
    public float Timer;
    public bool isStart = false;

    // 8 ������ Ŀ�����ϱ� ���� ����
    public GameObject stoneFront;
    public GameObject stoneFR;
    public GameObject stoneRight;
    public GameObject stoneBR;
    public GameObject stoneBack;
    public GameObject stoneBL;
    public GameObject stoneLeft;
    public GameObject stoneFL;

    // �����ϱ� ���ϱ� ���� List ����
    public List<GameObject> stoneList;

    // 8 ���⿡ ���� Ray
    private Ray rayFront;
    private Ray rayFR;
    private Ray rayRight;
    private Ray rayBR;
    private Ray rayBack;
    private Ray rayBL;
    private Ray rayLeft;
    private Ray rayFL;

    // Ray �ִ� �Ÿ�
    public float rayDistance;
    public float accel;
    public Vector2 toTarget;
    public Vector2 fromObstacle;

    // RayCast�� ���� ������
    private RaycastHit raycastHit;

    public EnvManager Env;

    private void Start()
    {
        carRigidbody = this.GetComponent<Rigidbody>();
        carController = this.GetComponent<CarController>();

        // CarController carController = this.gameObject.GetComponent<CarController>();
        stoneList = new List<GameObject>
        {
            // Stone���� stonList�� �߰�
            stoneFront,
            stoneFR,
            stoneRight,
            stoneBR,
            stoneBack,
            stoneBL,
            stoneLeft,
            stoneFL
        };
    }

    public override void OnEpisodeBegin()
    {
        isStart = false;
        Timer = 0f;
        carRigidbody.velocity = Vector3.zero;

        for (int i = 0; i < 2; i++)
        {
            if (Env.CurrentPaths.Count < 2)
            {
                Env.TargetIndices[0] = Env.CurrentPaths[0];
                Env.TargetIndices[1] = -1;
                break;
            }
            else
            {
                Env.TargetIndices[i] = Env.CurrentPaths[i];
            }
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.rotation.eulerAngles.y / 360);
        sensor.AddObservation(new Vector2(carRigidbody.velocity.x, carRigidbody.velocity.z));
        sensor.AddObservation(carController.steerAngle / carController.maxSteeringAngle);
        sensor.AddObservation(carController.isBraking);


        if (Env.TargetIndices[0] != -1)
        {
            Env.TargetPosition = Env.PathObjects[Env.TargetIndices[0]].transform.position;
            Env.FirstVector = new Vector2(Env.TargetPosition.x - this.transform.position.x, Env.TargetPosition.z - this.transform.position.z).normalized;

            Env.Distance = new Vector2(Env.TargetPosition.x - this.transform.position.x, Env.TargetPosition.z - this.transform.position.z).magnitude;
        }
        else
        {
            Env.TargetPosition = transform.position;
            Env.FirstVector = Vector2.zero.normalized;
        }

        sensor.AddObservation(new Vector2(transform.InverseTransformPoint(Env.TargetPosition).x, transform.InverseTransformPoint(Env.TargetPosition).z));
        sensor.AddObservation(Env.Distance);
        sensor.AddObservation(Env.FirstVector);

        if (Env.TargetIndices[1] != -1)
        {
            GameObject _ = Env.PathObjects[Env.TargetIndices[1]];
            Env.SecondVector = new Vector2(_.transform.position.x - this.transform.position.x, _.transform.position.z - this.transform.position.z).normalized;
        }
        else
        {
            Env.SecondVector = Vector2.zero.normalized;
        }

        sensor.AddObservation(Env.SecondVector);

        Env.Rays = CastRay();

        foreach(var ray in Env.Rays)
        {
            sensor.AddObservation(ray / rayDistance);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        Timer += Time.deltaTime;

        carController.horizontalInput = actions.ContinuousActions[0];
        carController.verticalInput = actions.ContinuousActions[1];
        if (actions.ContinuousActions[2] > 0)
        {
            carController.currentBrake = actions.ContinuousActions[2];
            carController.isBraking = true;
        }
        else
        {
            carController.isBraking = false;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continousActionsOut = actionsOut.ContinuousActions;
        continousActionsOut[0] = Input.GetAxis("Horizontal");
        continousActionsOut[1] = Input.GetAxis("Vertical");

        if (Input.GetKey(KeyCode.Space))
        {
            continousActionsOut[2] = 1;
        }
        else
        {
            continousActionsOut[2] = 0;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Obstacle"))
        {
            AddReward(-1);
            gameObject.SetActive(false);
            Env.StartEpisode();
            EndEpisode();
        }

        if (other.gameObject.CompareTag("Path"))
        {
            if (other.gameObject == Env.PathObjects[Env.CurrentPaths[0]])
            {
                if (isStart == false)
                {
                    isStart = true;
                    AddReward(1);
                }
                else
                {
                    if (Timer < 5f)
                    {
                        AddReward(5);
                    }
                    else
                    {
                        AddReward(1);
                    }
                }
                Timer = 0f;

                Env.CurrentPaths.RemoveAt(0);
                other.gameObject.SetActive(false);

                if (Env.CurrentPaths.Count == 0)
                {
                    gameObject.SetActive(false);
                    Env.StartEpisode();
                    EndEpisode();
                }
                else if (Env.CurrentPaths.Count == 1)
                {
                    Env.TargetIndices[0] = Env.CurrentPaths[0];
                    Env.TargetIndices[1] = -1;
                }
                else
                {
                    Env.TargetIndices[0] = Env.CurrentPaths[0];
                    Env.TargetIndices[1] = Env.CurrentPaths[1];
                }
            }
        }
    }

    /// <summary>
    /// ray�� �ð�ȭ
    /// </summary>
    private void DisplayRay(List<bool> hit, List<float> result)
    {
        if (hit.Count != 8) { Debug.LogError("Hit ���ڰ� �߸���"); return; }

        Ray[] rays = { rayFront, rayFR, rayRight, rayBR, rayBack, rayBL, rayLeft, rayFL };
        Color color;

        for (int index = 0; index < 8; index++)
        {
            color = hit[index] ? Color.red : Color.green;
            Debug.DrawRay(rays[index].origin, rays[index].direction * result[index], color);
        }
    }

    /// <summary>
    /// Ray Cast �� �Ÿ� ����, �ƹ��͵� �������� ���� ���, 10����
    /// </summary>
    private List<float> CastRay()
    {
        List<float> result = new List<float>();
        List<bool> hit = new List<bool>();
        Ray[] rays = new Ray[] { rayFront, rayFR, rayRight, rayBR, rayBack, rayBL, rayLeft, rayFL };

        UpdateRay();

        rayDistance = 10;

        int layer = LayerMask.GetMask("Obstacle");

        foreach (Ray ray in rays)
        {
            if (Physics.Raycast(ray, out raycastHit, rayDistance, layer))
            {
                result.Add((raycastHit.point - ray.origin).magnitude);
                hit.Add(true);
            }
            else
            {
                hit.Add(false);
                result.Add(10f);
            }
        }

        DisplayRay(hit, result);

        return result;
    }

    private void UpdateRay()
    {
        LinkStoneToRay(stoneFront, out rayFront);
        LinkStoneToRay(stoneFR, out rayFR);
        LinkStoneToRay(stoneRight, out rayRight);
        LinkStoneToRay(stoneBR, out rayBR);
        LinkStoneToRay(stoneBack, out rayBack);
        LinkStoneToRay(stoneBL, out rayBL);
        LinkStoneToRay(stoneLeft, out rayLeft);
        LinkStoneToRay(stoneFL, out rayFL);
    }

    /// <summary>
    /// stone�� ��ġ ���� �� forward�� Ȱ���Ͽ� ray�� �ʱ�ȭ�Ѵ�.
    /// </summary>
    /// <param name="_stone"></param>
    /// <param name="_ray"></param>
    private void LinkStoneToRay(GameObject _stone, out Ray _ray)
    {
        _ray = new Ray(_stone.transform.position, _stone.transform.forward);
    }

    private void OnDestroy()
    {
        Env = null;
    }
}
