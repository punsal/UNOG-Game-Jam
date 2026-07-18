using System;
using UnityEngine;

public enum EndingType { Dark, Light }

/// <summary>Chooses and reports the ending from the remaining light.</summary>
public class EndingResolver : MonoBehaviour
{
    public event Action<EndingType> RunEnded;

    public EndingType Resolve(float remainingLight)
    {
        EndingType ending = remainingLight <= 0f ? EndingType.Dark : EndingType.Light;
        Debug.Log($"Run ended with {ending} ending and {remainingLight:0.#} light.", this);
        GameAudio.Instance?.PlayEnding(ending == EndingType.Light ? EndingQuality.Good : EndingQuality.Bad);
        RunEnded?.Invoke(ending);
        return ending;
    }
}
