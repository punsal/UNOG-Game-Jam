using System;
using UnityEngine;

public enum EndingType { Dark, Light }

/// <summary>Chooses and reports the ending from the remaining light.</summary>
public class EndingResolver : MonoBehaviour
{
    // Dark when the run ends below this much relic light. Gate content offers
    // three -20 Light sacrifices, so the all-in Light route ends at 40 and any
    // lighter route ends at 60+; the threshold must sit between those totals.
    [SerializeField] private float darkEndingThreshold = 50f;

    public event Action<EndingType> RunEnded;

    public EndingType Resolve(float remainingLight)
    {
        EndingType ending = remainingLight < darkEndingThreshold ? EndingType.Dark : EndingType.Light;
        Debug.Log($"Run ended with {ending} ending: {remainingLight:0.#} light vs dark threshold {darkEndingThreshold:0.#}.", this);
        GameAudio.Instance?.PlayEnding(ending == EndingType.Light ? EndingQuality.Good : EndingQuality.Bad);
        RunEnded?.Invoke(ending);
        return ending;
    }
}
