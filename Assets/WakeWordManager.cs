using UnityEngine;
using Oculus.Voice;
using Meta.WitAi.Json;
using Meta.WitAi;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class WakeWordManager : MonoBehaviour
{
    public AppVoiceExperience appVoice;

    public GameObject voiceUI;
    public Image micVolume;

    public float micVolumeMultiplier = 5;

    public TextMeshProUGUI transcriptionText;

    public UnityEvent OnWakeWordDetected;
    public UnityEvent<WitResponseNode> OnResponseDetected;


    private bool voiceActivated = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        appVoice.VoiceEvents.OnResponse.AddListener(HandleResponse);

        appVoice.VoiceEvents.OnMicAudioLevelChanged.AddListener(UpdateVolumeUI);
        appVoice.VoiceEvents.OnPartialTranscription.AddListener(UpdateTranscriptionUI);

        voiceUI.SetActive(false);

        appVoice.Activate();
    }


    public void UpdateTranscriptionUI(string transcriptionValue)
    {
        if(voiceActivated)
            transcriptionText.text = transcriptionValue;
    }

    public void UpdateVolumeUI(float micvalue)
    {
        if(voiceActivated)
            micVolume.fillAmount = Mathf.Clamp01(micvalue * micVolumeMultiplier);
    }

    // Update is called once per frame
    public void HandleResponse(WitResponseNode witResponse)
    {
        Debug.Log(witResponse.GetTranscription() + "-" + witResponse.GetIntentName());

        if (voiceActivated)
        {
            Debug.Log("Voice Activated : ");
            voiceActivated = false;
            voiceUI.SetActive(false);
            OnResponseDetected.Invoke(witResponse);

        }
        else
        {
            if (witResponse.GetIntentName() == "wake_word")
            {
                voiceActivated = true;
                Debug.Log("Wake word detected");
                voiceUI.SetActive(true);
                transcriptionText.text = "";
                OnWakeWordDetected.Invoke();

            }
        }

        appVoice.Activate();

    }
}
