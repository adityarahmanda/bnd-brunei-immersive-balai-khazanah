// https://github.com/wangyangwang/hokuyo-unity

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LZY.Lidar
{
    interface IFilter<T>
    {
        T[] Filter(T[] input, int period);
    }
}