using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioDataBase", menuName = "Math Game Jam/Audio Database")]
public class AudioDataBaseSO : ScriptableObject//全部音频数据库
{
    public List<AudioClipData> player = new List<AudioClipData>();
    public List<AudioClipData> UIAudio = new List<AudioClipData>();
    public List<AudioClipData> mainMenuMusic = new List<AudioClipData>();
    public List<AudioClipData> levelMusic = new List<AudioClipData>();

    private Dictionary<string, AudioClipData> clipCollection;
    private bool collectionBuilt;

    private void BuildCollection()
    {
        if (collectionBuilt) return;

        clipCollection = new Dictionary<string, AudioClipData>();
        AddToCollection(player);
        AddToCollection(UIAudio);
        AddToCollection(mainMenuMusic);
        AddToCollection(levelMusic);
        collectionBuilt = true;
    }

    private void AddToCollection(List<AudioClipData> list)
    {
        foreach (var data in list)
        {
            if (data == null || string.IsNullOrEmpty(data.audioName)) continue;
            if (!clipCollection.ContainsKey(data.audioName))
            {
                clipCollection.Add(data.audioName, data);
            }
        }
    }

    public AudioClipData Get(string groupName)
    {
        BuildCollection();
        clipCollection.TryGetValue(groupName, out var data);
        return data;
    }

    private void OnEnable()
    {
        collectionBuilt = false;
    }
}