using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class AudioClipData//单条音频定义
{
    public string audioName;
    public List<AudioClip> clips = new List<AudioClip>();//音频列表
    [Range(0f, 1f)] public float maxVolume = 1f;//最大音量

    public AudioClip GetRandomClip()//获取随机音频
    {
        if (clips == null || clips.Count == 0) return null;
        return clips[Random.Range(0, clips.Count)];
    }
}