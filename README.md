# FieldAssistAR — XR Maintenance Assistant

A Unity-based mixed reality assistant that integrates **OpenAI vision & chat** and **Meta Wit voice recognition** to identify equipment and components in real time. Point your headset at a target object, speak a wake word, and get instant maintenance guidance, safety data, technical specifications, and operational status — spoken back via TTS.

Built as a proof-of-concept for hands-free, voice-driven field maintenance workflows on embedded XR hardware.

---

## Key Features

- **Multi-role mode selection** — Tailored experiences for `FieldTechnician`, `Inspector`, `Supervisor`, and `General` users, each with custom system prompts and conversation context
- **Voice-triggered interactions** — Wake-word listening powered by Meta Wit intent recognition
- **Vision-based component identification** — Captures frames and sends them to GPT-4o for image + text analysis to identify equipment and surface relevant technical data
- **Persistent conversation history** — Per-session memory scoped to the active user role
- **Text-to-speech output** — Spoken responses via a `TTSSpeaker` component for hands-free operation

---

## Tech Stack

| Layer | Technology |
|---|---|
| Engine | Unity 2020.3+ |
| AI / Vision | OpenAI GPT-4o (`OpenAIClient`) |
| Voice / NLU | Meta Wit.ai |
| Speech Output | Unity TTS (`TTSSpeaker`) |
| Camera Input | `WebCamTextureManager` |

---

## Quick Setup

### Prerequisites

- Unity **2020.3+** (check `ProjectSettings/ProjectVersion.txt` for the exact version used)
- OpenAI SDK imported into the Unity project
- Meta Wit package imported into the Unity project
- Microphone and webcam/passthrough access at runtime

### Steps

1. **Clone the repository**
   ```bash
   git clone https://github.com/pratushMukherjee/FragranceAR.git
   ```

2. **Open in Unity Hub** — Add the cloned folder as an existing project

3. **Configure the `AIManager` component** in the scene Inspector:

   | Field | Description |
   |---|---|
   | `OpenAIConfiguration` | Your OpenAI API key and model settings |
   | `WakeWordManager` | Wake-word and intent hooks |
   | `WebCamTextureManager` | Live camera / passthrough source |
   | `aiCanvas` / `loadingCanvas` / `modeSelectionCanvas` | UI panel references |
   | `aiResponseText` / `describePicture` | Text display elements |
   | `fieldTechButton` / `inspectorButton` / `supervisorButton` / `generalButton` | Role selection buttons |
   | `TTSSpeaker` | Speech output component |
   | `debugPicture` | *(Optional)* Static texture for Editor testing |

4. **Build for Android** — Switch platform to Android in Build Settings, connect your Meta Quest 3, and click **Build and Run**

---

## API Key Security

> **Never commit API keys to version control.**

- Set your OpenAI key in `Assets/Resources/OpenAIConfiguration.asset` locally via the Inspector
- This file should be listed in `.gitignore` or the key field left blank in tracked files
- Regenerate any key that was previously exposed in a commit

---

## Usage

1. Launch the app and select a **user role** from the role selection UI
2. Speak the configured **wake word** to begin listening
3. Meta Wit parses your voice input into one of the supported intents:

   | Intent | Description |
   |---|---|
   | `describe_vision` | Identify and describe the component in view |
   | `safety_check` | Retrieve safety and hazard information |
   | `storage_info` | Get storage and handling recommendations |
   | `usage_level` | Operating guidelines and tolerances |
   | `tech_specs` | Technical specifications and part data |
   | *(general)* | Free-form chat routed to the GPT-4o chat endpoint |

4. The assistant responds in text (`aiResponseText`) and speaks the response via TTS

> **Editor testing:** Populate `debugPicture` with a static texture to test vision prompts without a live camera.

---

## Architecture

```
Voice Input (Wit.ai STT) ──► Intent Router
                                  │
                    ┌──────┬──────┼──────┬──────┐
                    ▼      ▼      ▼      ▼      ▼
               describe  safety  storage usage  general
                    │      │      │      │      │
                    └──────┴──────┴──────┴──────┘
                                  │
                    Camera Frame + Role Context
                                  │
                                  ▼
                         GPT-4o Vision API
                        (image + text prompt)
                                  │
                                  ▼
                     TTS Output (spoken response)
```

- **Intent routing** — Wit.ai classifies voice input into discrete intents, each mapped to a specialized system prompt
- **Role-scoped context** — Each user role carries its own system prompt and conversation history, reset on role switch
- **Async orchestration** — API calls are non-blocking with configurable timeout handling to maintain responsiveness on embedded hardware

---

## Relevant Files

- **AI logic & integrations** — [`Assets/AIManager.cs`](Assets/AIManager.cs)

---

## Developer Notes

- Conversation history is stored **in-memory** (`conversationHistory`) and resets on role switch
- The project targets `Model.GPT4o` — update the model constant in `AIManager.cs` if needed
- Image + text prompts are sent as mixed-content `Message` objects for vision requests
- Role system prompts can be customized in `AIManager.cs` to target any equipment domain

---

## Troubleshooting

| Issue | Fix |
|---|---|
| Image capture returns null | Ensure camera is active and runtime permission is granted |
| OpenAI calls fail | Verify `OpenAIConfiguration` API key and network connectivity |
| Voice not triggering | Check microphone permissions and Wit.ai credentials in `ProjectSettings/wit.config` |
| Editor has no camera | Assign a texture to `debugPicture` for static testing |

---

## Contributing

Fork the repo and submit pull requests for bug fixes or features. Keep changes focused and add or update tests where applicable.

---

## License

Add your preferred license file to the repository (e.g. `LICENSE`).

---

## Author

**Pratush Mukherjee** · [GitHub](https://github.com/pratushMukherjee)
