import json
import os
import websockets

class RealtimeAI:
    def __init__(self):
        self.ws = None

    async def connect(self):
        self.ws = await websockets.connect(
    "wss://api.openai.com/v1/realtime"
    "?model=gpt-realtime-mini-2025-12-15",
            additional_headers={
                "Authorization": f"Bearer {os.getenv('OPENAI_API_KEY')}",
                "OpenAI-Beta": "realtime=v1",
            }
        )
        print("🤖 Connected to OpenAI Realtime")

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
