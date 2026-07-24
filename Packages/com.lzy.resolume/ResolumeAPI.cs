using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;
using UnityEngine.Networking;

namespace LZY.Resolume
{
    public static class ResolumeAPI
    {
        public static string BaseURL = "http://localhost:8080/api/v1/";

        public static async Task<Clip> GetClip(ClipRawData clipRawData)
        {
            return await GetClip(clipRawData.layer, clipRawData.column);
        }

        public static async Task<Clip> GetClip(int layer, int column)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(BaseURL + $"composition/layers/{layer}/clips/{column}"))
            {
                request.SetRequestHeader("accept", "application/json");
                var operation = request.SendWebRequest();

                while (!operation.isDone)
                    await Task.Yield();

                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
                {
                    throw new Exception(request.error);
                }

                Debug.Log(request.downloadHandler.text);
                return JsonConvert.DeserializeObject<Clip>(request.downloadHandler.text);
            }
        }
    }

    [Serializable]
    public class ClipRawData
    {
        public int layer = 2;
        public int column = 1;
        
        public override string ToString() => $"SceneData, Path:{GetVideoPath()}";
        
        public string GetVideoPath() => $"/composition/layers/{layer}/clips/{column}";
        
        public ClipRawData Clone()
        {
            return new ClipRawData(layer, column);
        }

        public ClipRawData CreateNextColumnClip()
        {
            return new ClipRawData(layer, column + 1);
        }
        
        public ClipRawData CreateNextLayerClip()
        {
            return new ClipRawData(layer + 1, column);
        }
        
        public ClipRawData() { }
        
        public ClipRawData(int layer, int column)
        {
            this.layer = layer;
            this.column = column;
        }
    }

    #region OSC
    public enum ConnectedStatus
    {
        Disconnected = 1,
        Previewed = 2,
        Connected = 3,
        PreviewedAndConnected = 4
    }
    #endregion
    
    #region Rest API
    public class Param { }
    
    [Serializable]
    public class ParamString : Param
    {
        public string valuetype;
        public long id;
        public string value;
    }

    [Serializable]
    public class ParamChoice : Param
    {
        public long id;
        public string valuetype;
        public string value;
        public int index;
        public List<string> options;
    }

    [Serializable]
    public class ParamTrigger : Param
    {
        public long id;
        public string valuetype;
        public bool value;
    }

    [Serializable]
    public class ParamState : Param
    {
        public long id;
        public string valuetype;
        public string value;
        public int index;
        public List<string> options;
    }
    
    [Serializable]
    public class ParamBoolean : Param
    {
        public string valuetype;
        public long id;
        public bool value;
    }

    [Serializable]
    public class ParamRange : Param
    {
        public long id;
        public string valuetype;
        public float min;
        public float max;
        public float @in;
        public float @out;
        public float value;
    }
    
    [Serializable]
    public class ParamView : Param
    {
        public string suffix;
        public string control_type;
        public float step;
        public string display_units;
        public float multiplier;
    }
    
    [Serializable]
    public class Transport
    {
        public ParamRange position;
        public TransportControls controls;
    }

    [Serializable]
    public class TransportControls
    {
        public ParamChoice playdirection;
        public ParamChoice playmode;
        public ParamChoice playmodeaway;
        public ParamRange duration;
        public ParamRange speed;
    }

    [Serializable]
    public class Clip
    {
        public long id;
        public ParamString name;
        public ParamChoice colorid;
        public ParamTrigger selected;
        public ParamState connected;
        public ParamChoice target;
        public ParamChoice triggerstyle;
        public ParamChoice beatsnap;
        public ParamChoice ignorecolumntrigger;
        public ParamChoice faderstart;
        public ParamChoice transporttype;
        public Transport transport;
        public Dictionary<string, Param> dashboard;
        public object audio;
        public Video video;
        public Thumbnail thumbnail;
    }
    
    [Serializable]
    public class Video
    {
        public ParamRange width;
        public ParamRange height;
        public ParamRange opacity;
        public object mixer;
        public List<VideoEffect> effects;
        public string description;
        public FileInfo fileinfo;
        public ParamBoolean r;
        public ParamBoolean g;
        public ParamBoolean b;
        public ParamBoolean a;
        public ParamChoice resize;
        public object sourceparams;
    }
    
    [Serializable]
    public class VideoEffect
    {
        public long id;
        public string name;
        public string display_name;
        public object mixer;
        public Dictionary<string, Param> @params;
    }

    [Serializable]
    public class FileInfo
    {
        public string path;
        public bool exists;
        public string duration;
        public int duration_ms;
        public FrameRate framerate;
        public int width;
        public int height;
    }

    [Serializable]
    public class FrameRate
    {
        public int num;
        public int denom;
    }

    [Serializable]
    public class Thumbnail
    {
        public int size;
        public string last_update;
        public bool is_default;
        public long id;
        public string path;
    }
    
    public class WebSocketData
    {
        public WebSocketAction action;
        public string path = String.Empty;
        public string parameter = String.Empty;
    }

    public class WebSocketMessage
    {
        public string id;
        public WebSocketMessageType type;
        public string path;
    }

    public class ChoiceParameter
    {
        public string valuetype;
        public string value;
        public int index;
        public string[] options;
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum WebSocketAction
    {
        post,
        reset,
        set,
        get,
        subscribe,
        unsubscribe,
        trigger
    }
    
    [JsonConverter(typeof(StringEnumConverter))]
    public enum WebSocketMessageType
    {
        unknown,
        sources_update,
        effects_update,
        thumbnail_update,
        parameter_update,
        parameter_subscribed
    }
    #endregion
}