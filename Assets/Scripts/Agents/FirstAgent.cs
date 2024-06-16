using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEditorInternal;

public class FirstAgent : Agent
{
    [SerializeField]
    private GameObject target
    {
        get
        {
            return mapManager.Target;
        }
    }
    [SerializeField]
    private List<float> rays;
    // Ray 최대 거리
    public float rayDistance = 10f;

    public Rigidbody carRigidbody;
    public CarController carController;

    // 8 방향을 커스텀하기 위한 지점
    public GameObject stoneFront;
    public GameObject stoneFR;
    public GameObject stoneRight;
    public GameObject stoneBR;
    public GameObject stoneBack;
    public GameObject stoneBL;
    public GameObject stoneLeft;
    public GameObject stoneFL;

    // 관리하기 편하기 위한 List 형태
    public List<GameObject> stoneList;

    // 8 방향에 대한 Ray
    private Ray rayFront;
    private Ray rayFR;
    private Ray rayRight;
    private Ray rayBR;
    private Ray rayBack;
    private Ray rayBL;
    private Ray rayLeft;
    private Ray rayFL;

    

    // RayCast를 위한 변수들
    private RaycastHit raycastHit;

    public MapManager mapManager;

    private void Start()
    {
        carRigidbody = GetComponent<Rigidbody>();
        carController = GetComponent<CarController>();

        stoneList = new List<GameObject>
        {
            // Stone들을 stonList에 추가
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
        carRigidbody.velocity = Vector3.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.rotation.eulerAngles.y / 360);
        sensor.AddObservation(new Vector2(carRigidbody.velocity.x, carRigidbody.velocity.z));
        sensor.AddObservation(carController.steerAngle / carController.maxSteeringAngle);
        sensor.AddObservation(carController.isBraking);

        Vector3 _position = transform.position;
        Vector3 _vector = Vector3.zero;
        
        if(target != null)
        {
            _position = transform.InverseTransformPoint(target.transform.position);
            _vector = target.transform.position - transform.position;
        }
        
        sensor.AddObservation(new Vector2(_position.x, _position.z));
        sensor.AddObservation(new Vector2(_vector.x, _vector.z).normalized);
        sensor.AddObservation(new Vector2(_vector.x, _vector.z).magnitude);

        rays = CastRay();

        foreach (var ray in rays)
        {
            sensor.AddObservation(ray / rayDistance);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        AddReward(-0.01f);

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
            mapManager.EndEpisode();
            //mapManager.StartEpisode();
            //EndEpisode();
        }

        if (other.gameObject.CompareTag("Path"))
        {
            if(other.gameObject == target)
            {
                AddReward(1);

                mapManager.OnArrival();
            }
        }
    }

    /// <summary>
    /// Ray Cast 후 거리 관측, 아무것도 관측되지 않은 경우, 10으로
    /// </summary>
    private List<float> CastRay()
    {
        List<float> result = new List<float>();
        List<bool> hit = new List<bool>();
        Ray[] rays = new Ray[] { rayFront, rayFR, rayRight, rayBR, rayBack, rayBL, rayLeft, rayFL };

        UpdateRay();

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

    /// <summary>
    /// ray를 시각화
    /// </summary>
    private void DisplayRay(List<bool> hit, List<float> result)
    {
        if (hit.Count != 8) { Debug.LogError("Hit 숫자가 잘못됨"); return; }

        Ray[] rays = { rayFront, rayFR, rayRight, rayBR, rayBack, rayBL, rayLeft, rayFL };
        Color color;

        for (int index = 0; index < 8; index++)
        {
            color = hit[index] ? Color.red : Color.green;
            Debug.DrawRay(rays[index].origin, rays[index].direction * result[index], color);
        }
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
    /// stone의 위치 정보 및 forward를 활용하여 ray를 초기화한다.
    /// </summary>
    /// <param name="_stone"></param>
    /// <param name="_ray"></param>
    private void LinkStoneToRay(GameObject _stone, out Ray _ray)
    {
        _ray = new Ray(_stone.transform.position, _stone.transform.forward);
    }

    private void OnDestroy()
    {
        mapManager = null;
    }
}
