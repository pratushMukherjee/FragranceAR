# XR AI Assistant

Overview
- Unity-based XR assistant that integrates OpenAI chat and Meta Wit voice intent recognition to identify IFF (International Flavors & Fragrances) products via webcam, provide safety, storage, usage, and scent-profile information, and speak responses using TTS.

Key Features
- Mode selection for different user roles: `LabTechnician`, `Perfumer`, `SalesTeam`, and `General`.
- Voice-triggered interactions via Meta Wit intents and wake-word listening.
- Image capture from webcam for vision-based prompts.
- Conversation history with system prompts tailored to the chosen user mode.
- Text-to-speech output using a `TTSSpeaker` component.

Prerequisites
- Unity 2020.3+ (or the project's targeted Unity version).
- OpenAI SDK and Meta Wit (installed/imported into the Unity project).
- Microphone and webcam access for runtime use.

Quick Setup
1. Open this project in Unity.
2. Locate the `AIManager` component in the scene (see `Assets/AIManager.cs` for implementation details).
3. In the Inspector, configure the following references on the `AIManager` GameObject:
   - `OpenAIConfiguration` (provide your OpenAI API key and settings).
   - `WakeWordManager` (wake-word and intent hooks).
   - `WebCamTextureManager` (webcam source).
   - UI references: `aiCanvas`, `loadingCanvas`, `modeSelectionCanvas`, `aiResponseText`, `describePicture`.
   - Mode selection buttons: `labTechButton`, `perfumerButton`, `salesButton`, `generalButton`.
   - `TTSSpeaker` for speech output.
   - `debugPicture` (optional) for Editor testing.

Notes on Usage
- Start the app and select a user mode from the mode selection UI.
- Speak the configured wake word to trigger listening; Meta Wit parses intents and transcription.
- Supported intents (examples): `describe_vision`, `safety_check`, `storage_info`, `usage_level`, `scent_profile`; general queries are sent to the chat endpoint.
- In the Unity Editor, `debugPicture` is used instead of the webcam for quick testing.

Developer Notes
- Conversation history is stored in-memory (`conversationHistory`) and reset when switching modes.
- The project uses `Model.GPT4o` for Chat requests via `OpenAIClient` — adjust the model constant if needed.
- Image + text prompts are sent as mixed content in `Message` objects when describing or analyzing products.

Troubleshooting
- If image capture returns null or wrong dimensions, ensure the webcam is active and permission is granted.
- If OpenAI calls fail, verify the `OpenAIConfiguration` API key and network connectivity.
- For Editor testing, populate `debugPicture` to avoid needing a live webcam.

Contributing
- Fork the repo and submit PRs for bug fixes or feature additions. Keep changes focused and add/update unit or integration tests where applicable.

License
- Add your preferred license file to the repository (e.g., `LICENSE`).

Relevant Files
- AI behavior and integration: [Assets/AIManager.cs](Assets/AIManager.cs)
