import json
import os
import websockets


SYSTEM_PROMPT = """
You are an XR assistant embedded in a mixed reality environment.
You are accompanying the user while they assemble a simple toy helicopter model.

The assembly consists only of these components:
- Chopper body with landing base
- Rotor blades
- Battery
- Tail

Your role is to:
- Explain the overall task when needed
- Talk naturally and keep the user oriented
- Answer questions, thoughts, or confusion
- Offer gentle context if the user seems unsure

Rules:
- NEVER respond to silence.
- Do NOT tell the user to attach or detach components.
- Do NOT invent new steps, tools, or instructions.
- Assume the user can see and interact with all objects in mixed reality.
- Be conversational, supportive, and present.

Guidance style:
- Speak as a helpful companion, not a trainer.
- You may describe what the current task involves at a high level.
- You may clarify what each component is for if asked.
- If the user expresses confusion, explain the task again calmly.
- Let the user control the pace and actions.

Your goal is to feel like a smart, friendly XR presence standing beside the user.
"""

class RealtimeAI:
    def __init__(self):
        self.ws = None

    async def connect(self):
       # api_key = os.getenv("OPENAI_API_KEY")
        api_key = os.getenv("OPENAI_API_KEY")
        self.ws = await websockets.connect(
            "wss://api.openai.com/v1/realtime?model=gpt-4o-realtime-preview-2024-12-17",
            additional_headers={
                "Authorization": f"Bearer {api_key}",
                "OpenAI-Beta": "realtime=v1",
            }
        )
        print("🤖 Connected to OpenAI Realtime")

        await self.ws.send(json.dumps({
            "type": "session.update",
            "session": {
                "instructions": SYSTEM_PROMPT
            }
        }))
        print("🤖 Connected to OpenAI Realtime with locked behavior")

    async def send_audio(self, base64_audio: str):
        await self.ws.send(json.dumps({
            "type": "input_audio_buffer.append",
            "audio": base64_audio
        }))

    async def commit_audio(self):
        await self.ws.send(json.dumps({
            "type": "input_audio_buffer.commit"
        }))

    async def receive(self):
        async for msg in self.ws:
            yield json.loads(msg)

    async def send_text(self, text):
        await self.ws.send(json.dumps({
            "type": "response.create",
            "response": {
                "modalities": ["audio", "text"],
                "instructions": text
            }
        }))

    async def cancel_response(self):
        await self.ws.send(json.dumps({
        "type": "response.cancel"
        }))
