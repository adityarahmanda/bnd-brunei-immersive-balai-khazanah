// https://github.com/wangyangwang/hokuyo-unity

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace LZY.Lidar
{
    public class RawObject
    {
        public int medianId { get { return idList[idList.Count / 2]; } }
        public int averageId { get { return (int)(idList.Average()); } }
        public double averageDist { get { return distancesList.Average(); } }
        public long medianDist { get { return distancesList[distancesList.Count / 2]; } }
        public float size
        {
            get
            {
                Vector2 pointA = GetPosition(cachedDirections[idList[0]], distancesList[0]);
                Vector2 pointB = GetPosition(cachedDirections[idList[idList.Count - 1]], distancesList[distancesList.Count - 1]);
                return Vector2.Distance(pointA, pointB);
            }
        }

        public List<long> distancesList;
        public List<int> idList;

        private readonly Vector2[] cachedDirections;
        private readonly Vector2[] cachedPoints;

        Vector2 _position = Vector2.zero;
        //position will be set once to save computing power
        bool positionSet = false;
        public Vector2 position
        {
            get
            {
                if (!positionSet) Debug.LogError("position has not bee set yet");
                return _position;
            }
            set { _position = value; }
        }

        public void CalculatePosition()
        {
            position = GetPosition(cachedDirections[medianId], medianDist);
            positionSet = true;
        }
        
        private Vector2 GetPosition()
        {
            return GetPosition(cachedDirections[medianId], medianDist);
        }

        private Vector2 GetPosition(Vector3 dir, float dist)
        {
            float x = dir.x * dist;
            float y = dir.y * dist;
            return new Vector2(x, y);
        }

        public RawObject(in Vector2[] cachedDirections, int id)
        {
            this.cachedDirections = cachedDirections;
            cachedPoints = new Vector2[cachedDirections.Length];
            distancesList = new List<long>();
            idList = new List<int>();
        }
    }
}
