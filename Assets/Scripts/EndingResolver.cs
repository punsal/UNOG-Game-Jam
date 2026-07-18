using System;
using UnityEngine;

public enum EndingType { Dark, Light }

public class EndingResolver : MonoBehaviour
{
    public event Action<EndingType> RunEnded;

    public EndingType Resolve(float remainingLight)
    {
        EndingType ending = remainingLight <= 0f ? EndingType.Dark : EndingType.Light;
        RunEnded?.Invoke(ending);
        return ending;
    }
}
