using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResolutionManager : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown; 
    public TMP_Dropdown displayModeDropdown;
    public Toggle vsyncToggle;
    private GameManager gameManager;
    public Resolution CurrentResolution;
    public FullScreenMode currentDisplayMode;
    
    Resolution[] resolutions;
    private readonly List<Resolution> selectedResolutionsList = new();

    private void Awake()
    {
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
    }

    private void Start()
    {
        resolutions = Screen.resolutions; 

        var resolutionStringlist = new List<string>();

        foreach (var res in resolutions)
        {
            var newResolution = res.width + "x" + res.height;

            if (resolutionStringlist.Contains(newResolution)) continue;
            resolutionStringlist.Add(newResolution);
            selectedResolutionsList.Add(res);
        }

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(resolutionStringlist);

        int currentIndex = 0;

        for (int i = 0; i < selectedResolutionsList.Count; i++)
        {
            if (selectedResolutionsList[i].width != Screen.width ||
                selectedResolutionsList[i].height != Screen.height) continue;
            currentIndex = i;
            break;
        }

        resolutionDropdown.value = currentIndex;
        resolutionDropdown.RefreshShownValue();
        
        displayModeDropdown.value = PlayerPrefs.GetInt("display_mode");
        displayModeDropdown.RefreshShownValue();
        
        if (PlayerPrefs.GetInt("resolution_vsync") == 1)
        {
            vsyncToggle.isOn = true;
            GameObject.Find("Vsync Toggle Text").GetComponent<TextMeshProUGUI>().text = "ON";
        }
        else
        {
            vsyncToggle.isOn = false;
            GameObject.Find("Vsync Toggle Text").GetComponent<TextMeshProUGUI>().text = "OFF";
        }
    }

    public void SetResolution(int selectedResolution)
    {

        Screen.SetResolution(
            selectedResolutionsList[selectedResolution].width,
            selectedResolutionsList[selectedResolution].height,
            Screen.fullScreen
        );
        CurrentResolution = selectedResolutionsList[selectedResolution];
        gameManager.SaveResolution(CurrentResolution);
    }
    
    public void SetDisplayMode(int selectedDisplayMode)
    {
        Screen.fullScreenMode = selectedDisplayMode switch
        {
            // Exclusive full screen is windows exclusive, fall back to fullscreenwindow on macos & linux
            0 => Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor
                ? FullScreenMode.ExclusiveFullScreen
                : FullScreenMode.FullScreenWindow,
            1 => FullScreenMode.FullScreenWindow,
            2 => FullScreenMode.Windowed,
            _ => Screen.fullScreenMode
        };
        currentDisplayMode = Screen.fullScreenMode;
        gameManager.SaveDisplayMode(selectedDisplayMode);
    }

    public void SetVsync(bool selectedVsync)
    {
        QualitySettings.vSyncCount = selectedVsync ? 1 : 0;
        GameObject.Find("Vsync Toggle Text").GetComponent<TextMeshProUGUI>().text = selectedVsync ? "ON" : "OFF";
        AudioManager.PlaySound(selectedVsync ? SoundType.UIClickIn : SoundType.UIClickOut);
        gameManager.SaveVsync(selectedVsync ? 1 : 0);
    }
}