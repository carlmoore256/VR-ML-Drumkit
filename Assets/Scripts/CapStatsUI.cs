using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CapStatsUI : MonoBehaviour
{
    public Text m_FrameStat;
    public Text m_TimeStat;
    public Text m_SampleRateStat;

    // updates stats that won't change during capture
    public void UpdateStaticStats(int sr)
    {
        m_SampleRateStat.text = $"{sr} Hz";
    }

    // updates visible stats on the display
    public void UpdateStats(int frames, float time)
    {
        m_FrameStat.text = frames.ToString();
        m_TimeStat.text = time.ToString();
    }
}
