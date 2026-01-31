# AI-Powered Mixed Reality Assembly Assistant

## Overview
This project is an **AI-powered Mixed Reality (MR) prototype** that explores the feasibility of integrating **conversational AI with hands-free MR interaction** for procedural assembly tasks. The system combines **Unity-based MR**, a **Python FastAPI backend**, and **OpenAI’s conversational models** to provide real-time, voice-based guidance and spatial task assistance.

The project focuses on **system architecture, real-time communication, and interaction design**, serving as a technical foundation for future usability and evaluation studies in training and industrial contexts.

Mixed Reality enables spatial visualization, while conversational AI offers natural interaction. This project investigates whether **AI-guided MR systems can deliver intuitive, hands-free task assistance** without compromising system stability or responsiveness.

---

## System Architecture
The system consists of three main components:

1. **Unity MR Client**
   - Captures microphone input
   - Streams audio data to backend

2. **Python FastAPI Backend**
   - Manages WebSocket communication
   - Streams audio to OpenAI’s API
   - Routes AI-generated responses back to Unity
   - Maintains session state and task events

3. **OpenAI Conversational AI**
   - Processes voice input
   - Generates contextual, task-aware responses

Bidirectional WebSocket communication enables low-latency streaming between all components.

---

## Interaction Design
- **Voice-based commands** for hands-free guidance
- **Hand tracking** for object manipulation
- Smart microphone control pauses input during AI speech to prevent audio feedback

---

## Technical Stack
- **Unity 6**
- **Mixed Reality Toolkit (MRTK)**
- **XR Interaction Toolkit**
- **C# (MR client, audio streaming, interaction logic)**
- **Python (FastAPI, async WebSockets)**
- **OpenAI API (Conversational AI)**
- **JSON-based data exchange**
- **Hand tracking for gesture interaction**

---

## Implementation Details
### Backend (Python)
- Asynchronous FastAPI server
- Bidirectional WebSocket handling
- Concurrent audio streaming and response routing
- Modular design with session state tracking and audio buffer management

### Frontend (Unity)
- Microphone capture and PCM16 audio encoding
- 24kHz audio streaming to backend
- Asynchronous reception of AI responses
- Spatial audio playback in MR environment
- Event-based notifications for object interactions

---

## Results & Observations
- Successfully demonstrated **functional AI–MR integration**
- Real-time voice interaction achieved under stable network conditions
- End-to-end response latency ranged from **~500ms to 2000ms**, depending on internet connectivity
- System architecture proved scalable but sensitive to network reliability

> ⚠️ This project focused on **technical feasibility**, not formal user studies or performance benchmarking.

---

## Limitations
- Network-dependent latency
- No formal usability or learning outcome evaluation
- Limited task complexity
- Cloud deployment introduces response variability

---

## Demo
🎥 **Video Demo:**  
https://youtube.com/shorts/9xRV02SQ2n0

---

## Author
**Muhammad Ehtesham Safeer**  
XR Developer | Applied XR & Intelligent Systems  
📧 Email: ehteshamsafeer@gmail.com  
🔗 LinkedIn: https://www.linkedin.com/in/ehteshamsafeer/

---

## Disclaimer
This project is a **research prototype** developed to explore system architecture and interaction design. It is **not intended for production or safety-critical deployment** without extensive validation and evaluation.
