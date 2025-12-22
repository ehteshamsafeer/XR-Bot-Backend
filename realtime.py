import json
import os
import websockets

class RealtimeAI:
    def __init__(self):
        self.ws = None

    async def connect(self):
        OPENAI_API_KEY="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"
        print("🔑 OPENAI_API_KEY detected:", OPENAI_API_KEY)
        self.ws = await websockets.connect(
    "wss://api.openai.com/v1/realtime"
    "?model=gpt-realtime-mini-2025-12-15",
            additional_headers={
                "Authorization": f"Bearer {OPENAI_API_KEY}",
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
