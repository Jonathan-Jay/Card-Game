using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdaptiveMusic : MonoBehaviour
{
    // Start is called before the first frame update
    public PlayerData player1;
    public PlayerData player2;

    public List<AudioSource> music;

    public int[] hpPlay = new int[] {18, 16, 14, 12, 10, 0};

    int segementIndex = -1;
    void OnEnable() 
    {
        player1.healthUpdated += UpdateMusic;    
        player2.healthUpdated += UpdateMusic;    
    }

    void UpdateMusic(int previousHP)
    {
        float lowestHP = Mathf.Min(player1.currentHP, player2.currentHP);
        for (int i = 0; i < hpPlay.Length; i++)
        {
            if(lowestHP >= hpPlay[i])
            {
                if(segementIndex != i)
                {
                    //Debug.Log("play " + i);
                    //Play segment 1
                    if (target < 0)
                        StartCoroutine(StartNewSegment(i));
                    else
                        target = segementIndex = i;
                }
                break;
            }
        }
    }

    int target = -1;
    private IEnumerator StartNewSegment(int index)
    {
        //Not great not terrible.
        int prev = segementIndex;
        target = segementIndex = index;

        float length = 0;
        if (prev >= 0)
        {
            length = music[prev].clip.length - music[prev].time;
            yield return new WaitForSeconds(length);
            music[prev]. Stop();
        }
        music[target].Play();
        target = -1;
    }


}

