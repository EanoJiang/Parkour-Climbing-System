using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ClimbPoint : MonoBehaviour
{
    [SerializeField] bool mountPoint;
    [SerializeField] List<Neighbour> neighbours;

    //只要攀岩架之间是邻居，那就自动创建双向关系
    public void Awake()
    {
        //只对标记为双向连接的邻居创建双向连接
        var twoWayNeighbours = neighbours.Where(n => n.isTwoWay);
        foreach (var neighbour in twoWayNeighbours)
        {
            neighbour.point?.CreateConnection(this, -neighbour.direction, neighbour.connectionType, neighbour.isTwoWay);
        }
    }

    public void CreateConnection(ClimbPoint point, Vector2 direction, ConnectionType connectionType,
                            bool isTwoWay = true)
    {
        var neighbour = new Neighbour()
        {
            point = point,
            direction = direction,
            isTwoWay = isTwoWay,
            connectionType = connectionType
        };
        neighbours.Add(neighbour);
    }

    //获取邻居攀岩架
    public Neighbour GetNeighbour(Vector2 direction)
    {
        Neighbour neighbour = null;
        if (direction.y != 0)
            //找到第一个y方向匹配的neighbour
            neighbour = neighbours.FirstOrDefault(n => n.direction.y == direction.y);
        //如果在y轴没找到匹配的neighbour
        if (neighbour == null && direction.x != 0)
            //找到第一个x方向匹配的neighbour
            neighbour = neighbours.FirstOrDefault(n => n.direction.x == direction.x);
        return neighbour;
    }

    private void OnDrawGizmos()
    {
        Debug.DrawRay(transform.position, transform.forward, Color.blue);
        foreach (var neighbour in neighbours)
        {
            if (neighbour.point != null)
            {
                Debug.DrawLine(transform.position, neighbour.point.transform.position,
                         (neighbour.isTwoWay) ? Color.green : Color.gray);
            }

        }
    }

    public bool MountPoint => mountPoint;
}

//下面的序列化字段可见
[System.Serializable]
public class Neighbour
{
    public ClimbPoint point;
    public Vector2 direction;
    public bool isTwoWay;
    public ConnectionType connectionType;

}
public enum ConnectionType {
    Jump,
    Move,
}
