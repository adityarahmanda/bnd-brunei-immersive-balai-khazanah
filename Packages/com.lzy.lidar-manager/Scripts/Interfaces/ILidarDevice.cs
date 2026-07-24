// https://github.com/wangyangwang/hokuyo-unity

using System.Collections.Generic;
using UnityEngine.Events;

namespace LZY.Lidar
{
	public interface ILidarDevice 
	{
		public List<long> distances { get; set; }
		public List<long> strengths { get; set; }
		public float minDegreeScope { get; }
		public float maxDegreeScope { get; }

		public bool isConnected { get; }
		public UnityEvent<bool> onConnectionChanged { get; }
		
		public void Disconnect();
	}
}
