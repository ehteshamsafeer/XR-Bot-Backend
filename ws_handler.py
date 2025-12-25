import asyncio
import json
from fastapi import WebSocket
from realtime import RealtimeAI


async def handle_unity_ws(ws: WebSocket):
    current_step = 1
    await ws.accept()
    print("✅ Unity connected")

    ai = RealtimeAI()
    await ai.connect()

    audio_chunks = []
    response_requested = False
    async def send_step_instruction():
        nonlocal current_step

        if current_step == 1:
            await ai.send_text(
                "Step one. Place the connector on the base."
            )

        elif current_step == 2:
            await ai.send_text(
                "Step two. Place the cap on top of the connector."
            )

        elif current_step == 3:
            await ai.send_text(
                "Assembly complete. Good work."
            )

    await send_step_instruction()


    async def unity_to_ai():
        nonlocal response_requested
        nonlocal current_step

        try:
            while True:
                msg = await ws.receive_text()
                data = json.loads(msg)
                if data["type"] == "step_completed":
                    current_step += 1
                    await send_step_instruction()

                if data["type"] == "audio_input":
                    # Send mic audio to OpenAI
                    await ai.send_audio(data["data"])

                    # Auto-commit & request response ONCE
                    if not response_requested:
                        response_requested = True

                        await ai.commit_audio()

                        await ai.ws.send(json.dumps({
                            "type": "response.create",
                            "response": {
                                "modalities": ["audio"],
                                "instructions": "Respond naturally to the user."
                            }
                        }))
        except Exception as e:
            print("❌ Unity disconnected:", e)

    async def ai_to_unity():
        nonlocal audio_chunks, response_requested

        try:
            async for event in ai.receive():
                event_type = event.get("type")
                print("⬅️ OpenAI event:", event_type)

                if event_type == "response.audio.delta":
                    audio_chunks.append(event["delta"])

                elif event_type == "response.audio.done":
                    full_audio = "".join(audio_chunks)

                    await ws.send_text(json.dumps({
                        "type": "audio_output",
                        "data": full_audio
                    }))

                    # Reset for next utterance
                    audio_chunks.clear()
                    response_requested = False

        except Exception as e:
            print("❌ OpenAI disconnected:", e)

    await asyncio.gather(
        unity_to_ai(),
        ai_to_unity()
    )
