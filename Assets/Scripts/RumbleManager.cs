using System;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class RumbleManager : MonoBehaviour
{
    public static RumbleManager Instance;
    public Gamepad Gamepad;
    private void Awake()
    {
        if (!Instance)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void Rumble(float low, float high, float duration)
    {
        Gamepad.SetMotorSpeeds(low, high);
        //Invoke(nameof(StopRumble), duration);
    }

    private void StopRumble()
    {
        Gamepad.SetMotorSpeeds(0, 0);
    }
    
    public void TestRumble()
    {
        if (Gamepad is null)
        {
            Debug.Log("No gamepad");
            return;
        }
        Debug.Log("Rumbling " + Gamepad.name);
        Rumble(0.5f, 0.5f, 1f);
    }
}
