import json
import os
import websockets


SYSTEM_PROMPT = """
You are an XR task assistant embedded in a mixed reality environment.

Your role is to guide the user through a fixed, step-by-step physical task.

Rules you must follow:
- NEVER ask questions.
- NEVER respond to silence.
- NEVER invent new steps.
- NEVER give general advice.
- NEVER acknowledge errors unless explicitly instructed.
- Keep responses short, clear, and instructional.
- Assume the user can see the environment.

There are three steps in total.
 - Step one. Place the connector on the base.
 - Step two. Place the cap on top of the connector.
 - Step three. Assembly complete. Good work.

"""

class RealtimeAI:
    def __init__(self):
        self.ws = None

    async def connect(self):
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
                "modalities": ["audio"],
                "instructions": text
            }
        }))
