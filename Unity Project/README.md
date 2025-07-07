# Virtual Agent for Robotic Ultrasound Patient

This is the Unity project for virtual agent for robotic ultrasound patient.

## How to run

There are 2 scenes in the project. 

- Virtual Agent for Patient (IEEE VR): This is the AR condition of the IEEE VR paper, and could be deployed onto Meta Quest 3.
- IVS for Patient (MICCAI): This is the Patient-Facing IVS of the MICCAI paper, and could be deployed onto Meta Quest 3, this should be used together with the [Physician-Facing IVS](https://github.com/stytim/IVS/tree/main/Physician-IVS). 

## Prerequisite

Make sure to have these installed on your compute server and setup the IP address and port correctly

- STT: [RealtimeSTT](https://github.com/KoljaB/RealtimeSTT)
- LLM: [llamalib](https://github.com/undreamai/LlamaLib/releases/tag/v1.2.5)
- TTS: [Kokoro-FastAPI](https://github.com/remsky/Kokoro-FastAPI)


## Disclaimer
Due to licensing issues with some third-party plugins and libraries, we are unable to share certain components on GitHub. Specifically, the following plugins used in the original paper are not included in this repository:

- [**Final-IK**](https://assetstore.unity.com/packages/tools/animation/final-ik-14290).
- [**SALSA LipSync Suite tool**](https://assetstore.unity.com/packages/tools/animation/salsa-lipsync-suite-148442)

If you require these plugins for your work, please refer to the official sources for obtaining them.


