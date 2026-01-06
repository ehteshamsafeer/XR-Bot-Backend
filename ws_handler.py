import asyncio
import json
from fastapi import WebSocket
from realtime import RealtimeAI
ai_is_speaking = False

async def handle_unity_ws(ws: WebSocket):
    await ws.accept()
    print("✅ Unity connected")

    ai = RealtimeAI()
    await ai.connect()

    audio_chunks = []
    response_requested = False
    async def send_step_instruction(attached_obj):
        instruction = (
            f"{attached_obj}"
            f"Let's proceed to the next step."
            )

        # 1️⃣ Send instruction text
        await ai.send_text(instruction)

        # 2️⃣ Explicitly request a spoken response
       # await ai.send_text({
        #    "type": "response.create",
         #   "response": {
          #      "modalities": ["audio", "text"],
           #     "instructions": "Respond naturally to the user."
           # }
       # })
        
    #await send_step_instruction(data["message"])


    async def unity_to_ai():
        nonlocal response_requested

        try:
            while True:
                msg = await ws.receive_text()
                data = json.loads(msg)
                if data["type"] == "task_completed":
                    #await ai.cancel_response()
                    await send_step_instruction(data["message"])

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
                await ws.send_text(json.dumps({
                        "type": "ai_done",
                        "data": "Resume Mic Streaming"
                    }))

        except Exception as e:
            print("❌ OpenAI disconnected:", e)

    await asyncio.gather(
        unity_to_ai(),
        ai_to_unity()
    )
