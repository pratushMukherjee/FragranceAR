using UnityEngine;
using OpenAI;
using OpenAI.Models;
using OpenAI.Chat;
using System.Collections.Generic;
using System.Threading.Tasks;
using Meta.WitAi.Json;
using Meta.WitAi;
using PassthroughCameraSamples;
using TMPro;
using UnityEngine.UI;
using Meta.WitAi.TTS.Utilities;

public enum UserMode
{
    LabTechnician,
    Perfumer,
    SalesTeam,
    General
}

public class AIManager : MonoBehaviour
{
    public OpenAIConfiguration openAIconfiguration;
    public WakeWordManager wakeWordManager;
    public WebCamTextureManager webcamManager;

    [Header("UI Components")]
    public GameObject aiCanvas;
    public GameObject loadingCanvas;
    public TextMeshProUGUI aiResponseText;
    public RawImage describePicture;
    
    [Header("Mode Selection UI")]
    public GameObject modeSelectionCanvas;
    public Button labTechButton;
    public Button perfumerButton;
    public Button salesButton;
    public Button generalButton;
    public TextMeshProUGUI currentModeText;

    public Texture2D debugPicture;
    public TTSSpeaker ttsSpeaker;
    
    private OpenAIClient openAI;
    private Texture2D picture;
    private UserMode currentMode = UserMode.General;
    private List<Message> conversationHistory;

    void Start()
    {
        openAI = new OpenAIClient(openAIconfiguration);
        wakeWordManager.OnResponseDetected.AddListener(HandleResponse);
        wakeWordManager.OnWakeWordDetected.AddListener(ResetUI);

        // Initialize conversation history with system prompt
        conversationHistory = new List<Message>
        {
            new Message(Role.System, GetSystemPrompt())
        };

        // Setup mode selection buttons
        labTechButton.onClick.AddListener(() => SetUserMode(UserMode.LabTechnician));
        perfumerButton.onClick.AddListener(() => SetUserMode(UserMode.Perfumer));
        salesButton.onClick.AddListener(() => SetUserMode(UserMode.SalesTeam));
        generalButton.onClick.AddListener(() => SetUserMode(UserMode.General));

        aiCanvas.SetActive(false);
        loadingCanvas.SetActive(false);
        modeSelectionCanvas.SetActive(true); // Show mode selection at start
        
        UpdateModeDisplay();
    }

    public void SetUserMode(UserMode mode)
    {
        currentMode = mode;
        UpdateModeDisplay();
        modeSelectionCanvas.SetActive(false);
        
        // Reset conversation history with new system prompt when mode changes
        conversationHistory.Clear();
        conversationHistory.Add(new Message(Role.System, GetSystemPrompt()));
    }

    public void ShowModeSelection()
    {
        modeSelectionCanvas.SetActive(true);
        ResetUI();
    }

    private void UpdateModeDisplay()
    {
        currentModeText.text = "Current Mode: " + currentMode.ToString().Replace("_", " ");
    }

    public void ResetUI()
    {
        aiCanvas.SetActive(false);
        loadingCanvas.SetActive(false);
    }

    public async void HandleResponse(WitResponseNode response)
    {
        string intent = response.GetIntentName();
        string transcription = response.GetTranscription().ToLower();
        string result;

        loadingCanvas.SetActive(true);

        // Handle different intents
        if (intent == "describe_vision" || transcription.Contains("identify"))
        {
            result = await DescribeChatGPT();
            Debug.Log("CHAT GPT Response: " + result);
            UpdateResultUI(result, picture);
        }
        else if (intent == "safety_check" || transcription.Contains("safety"))
        {
            result = await GetSafetyInfo();
            Debug.Log("Safety Response: " + result);
            UpdateResultUI(result, picture);
        }
        else if (intent == "storage_info" || transcription.Contains("storage") || transcription.Contains("store"))
        {
            result = await GetStorageInfo();
            Debug.Log("Storage Response: " + result);
            UpdateResultUI(result, picture);
        }
        else if (intent == "usage_level" || transcription.Contains("usage") || transcription.Contains("dosage"))
        {
            result = await GetUsageInfo();
            Debug.Log("Usage Response: " + result);
            UpdateResultUI(result, picture);
        }
        else if (intent == "scent_profile" || transcription.Contains("scent") || transcription.Contains("profile"))
        {
            result = await GetScentProfile();
            Debug.Log("Scent Profile Response: " + result);
            UpdateResultUI(result, picture);
        }
        else
        {
            result = await AskChatGPT(response.GetTranscription());
            Debug.Log("General CHAT GPT Response: " + result);
            UpdateResultUI(result, null);
        }

        ttsSpeaker.Speak(result);
    }

    public void UpdateResultUI(string resultText, Texture2D resultTexture)
    {
        loadingCanvas.SetActive(false);
        aiCanvas.SetActive(true);

        aiResponseText.text = resultText;

        if (resultTexture)
        {
            describePicture.enabled = true;
            describePicture.texture = resultTexture;
        }
        else
        {
            describePicture.enabled = false;
        }
    }

    private string GetSystemPrompt()
    {
        string basePrompt = "You are an expert AI assistant specializing in International Flavors & Fragrances (IFF) products. You have deep knowledge of fragrance and flavor compounds, raw materials, safety protocols, and industry standards.";

        switch (currentMode)
        {
            case UserMode.LabTechnician:
                return basePrompt + " Focus on safety protocols, handling procedures, storage requirements, chemical properties, and quality control measures. Always prioritize safety information and proper handling techniques.";
            
            case UserMode.Perfumer:
                return basePrompt + " Focus on scent profiles, fragrance families, blending properties, olfactory characteristics, creative applications, and artistic aspects of fragrance creation. Provide detailed sensory descriptions.";
            
            case UserMode.SalesTeam:
                return basePrompt + " Focus on product applications, market positioning, customer benefits, usage levels in different applications, competitive advantages, and commercial aspects. Make information accessible to non-technical audiences.";
            
            case UserMode.General:
            default:
                return basePrompt + " Provide comprehensive information covering identification, basic properties, common applications, and general usage guidelines.";
        }
    }

    public async Task<string> AskChatGPT(string input)
    {
        var messages = new List<Message>
        {
            new Message(Role.System, GetSystemPrompt()),
            new Message(Role.User, input)
        };

        var request = new ChatRequest(messages, model: Model.GPT4o);
        var result = await openAI.ChatEndpoint.GetCompletionAsync(request);

        return result.FirstChoice.Message.ToString();
    }

    public async Task<string> DescribeChatGPT()
    {
        if (Application.isEditor)
        {
            picture = debugPicture;
        }
        else
        {
            var cam = webcamManager.WebCamTexture;
            if (!cam.isPlaying) cam.Play();
            if (picture == null || picture.width != cam.width || picture.height != cam.height)
                picture = new Texture2D(cam.width, cam.height, TextureFormat.RGB24, false);

            picture.SetPixels32(cam.GetPixels32());
            picture.Apply(false);
        }

        // Add user's image + question to history
        conversationHistory.Add(new Message(Role.User, new List<Content>
        {
            "Identify this IFF product or material. Provide detailed information relevant to my role. Include product identification, key characteristics, and relevant technical details.", picture
        }));

        // Send the whole history
        var request = new ChatRequest(conversationHistory, model: Model.GPT4o);
        var result = await openAI.ChatEndpoint.GetCompletionAsync(request);

        // Get reply text
        string reply = result.FirstChoice.Message.ToString();

        // Add assistant reply to history
        conversationHistory.Add(new Message(Role.Assistant, reply));

        return reply;
    }

    public async Task<string> GetSafetyInfo()
    {
        if (Application.isEditor)
        {
            picture = debugPicture;
        }
        else
        {
            var cam = webcamManager.WebCamTexture;
            if (!cam.isPlaying) cam.Play();
            if (picture == null || picture.width != cam.width || picture.height != cam.height)
                picture = new Texture2D(cam.width, cam.height, TextureFormat.RGB24, false);

            picture.SetPixels32(cam.GetPixels32());
            picture.Apply(false);
        }

        // Add safety-specific request to history
        conversationHistory.Add(new Message(Role.User, new List<Content>
        {
            "What are the safety requirements and handling procedures for this product? Include PPE requirements, hazard warnings, and emergency procedures.", picture
        }));

        var request = new ChatRequest(conversationHistory, model: Model.GPT4o);
        var result = await openAI.ChatEndpoint.GetCompletionAsync(request);

        string reply = result.FirstChoice.Message.ToString();
        conversationHistory.Add(new Message(Role.Assistant, reply));

        return reply;
    }

    public async Task<string> GetStorageInfo()
    {
        if (Application.isEditor)
        {
            picture = debugPicture;
        }
        else
        {
            var cam = webcamManager.WebCamTexture;
            if (!cam.isPlaying) cam.Play();
            if (picture == null || picture.width != cam.width || picture.height != cam.height)
                picture = new Texture2D(cam.width, cam.height, TextureFormat.RGB24, false);

            picture.SetPixels32(cam.GetPixels32());
            picture.Apply(false);
        }

        conversationHistory.Add(new Message(Role.User, new List<Content>
        {
            "What are the proper storage requirements for this product? Include temperature, humidity, light exposure, shelf life, and any special storage considerations.", picture
        }));

        var request = new ChatRequest(conversationHistory, model: Model.GPT4o);
        var result = await openAI.ChatEndpoint.GetCompletionAsync(request);

        string reply = result.FirstChoice.Message.ToString();
        conversationHistory.Add(new Message(Role.Assistant, reply));

        return reply;
    }

    public async Task<string> GetUsageInfo()
    {
        if (Application.isEditor)
        {
            picture = debugPicture;
        }
        else
        {
            var cam = webcamManager.WebCamTexture;
            if (!cam.isPlaying) cam.Play();
            if (picture == null || picture.width != cam.width || picture.height != cam.height)
                picture = new Texture2D(cam.width, cam.height, TextureFormat.RGB24, false);

            picture.SetPixels32(cam.GetPixels32());
            picture.Apply(false);
        }

        conversationHistory.Add(new Message(Role.User, new List<Content>
        {
            "What are the typical usage levels and application methods for this product? Include recommended dosages for different applications and formulation guidelines.", picture
        }));

        var request = new ChatRequest(conversationHistory, model: Model.GPT4o);
        var result = await openAI.ChatEndpoint.GetCompletionAsync(request);

        string reply = result.FirstChoice.Message.ToString();
        conversationHistory.Add(new Message(Role.Assistant, reply));

        return reply;
    }

    public async Task<string> GetScentProfile()
    {
        if (Application.isEditor)
        {
            picture = debugPicture;
        }
        else
        {
            int width = webcamManager.WebCamTexture.width;
            int height = webcamManager.WebCamTexture.height;

            if (picture == null || picture.width != width || picture.height != height)
            {
                picture = new Texture2D(width, height, TextureFormat.RGB24, false);
            }

            Color32[] pixels = new Color32[width * height];
            webcamManager.WebCamTexture.GetPixels32(pixels);
            picture.SetPixels32(pixels);
            picture.Apply();
        }

        var messages = new List<Message>
        {
            new Message(Role.System, "You are a master perfumer and olfactory expert. Focus on detailed scent profiles, fragrance families, note breakdowns, olfactory characteristics, and blending properties."),
            new Message(Role.User, new List<Content>{
                "Describe the scent profile of this product. Include fragrance family, top/middle/base notes, olfactory characteristics, and how it might blend with other ingredients.", picture
            })
        };

        var request = new ChatRequest(messages, model: Model.GPT4o);
        var result = await openAI.ChatEndpoint.GetCompletionAsync(request);

        return result.FirstChoice.Message.ToString();
    }
}