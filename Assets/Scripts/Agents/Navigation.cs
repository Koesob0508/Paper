using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class Navigation : Agent
{
    private bool _on = false;
    public bool On { get { return _on; } }
    public MapManager mapManager;

    public GameObject Target
    {
        get
        {
            if(_on)
            {
                return gameObject;
            }
            else
            {
                return mapManager.Target;
            }
        }
    }

    public override void OnEpisodeBegin()
    {
        
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //Vector3 relativePosition = mapManager.Agent.transform.InverseTransformPoint(mapManager.Target.transform.position);
        //sensor.AddObservation(relativePosition);  // 상대적 위치 추가 (3개의 관찰값)

        //// 오브젝트 B의 회전을 오브젝트 A의 로컬 좌표계로 변환하여 상대적 회전을 계산
        //Quaternion relativeRotation = Quaternion.Inverse(mapManager.Agent.transform.rotation) * mapManager.Target.transform.rotation;
        //sensor.AddObservation(relativeRotation);

        //foreach(var _ray in mapManager.Agent.GetComponent<FirstAgent>().Rays)
        //{
        //    sensor.AddObservation(_ray);
        //}
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        
    }
}