using UnityEngine;
using System.Collections;

public class StartTimer : MonoBehaviour
{
    private IEnumerator Start()
    {
        Debug.Log("StartTimer started");
        yield return null;
    }
}