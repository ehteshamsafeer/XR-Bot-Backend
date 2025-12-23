import json
import os
import websockets

class RealtimeAI:
    def __init__(self):
        self.ws = None

    async def connect(self):
        self.ws = await websockets.connect(
            "wss://api.openai.com/v1/realtime?model=gpt-4o-realtime-preview-2024-12-17",
            additional_headers={
                "Authorization": "Bearer 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",
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
