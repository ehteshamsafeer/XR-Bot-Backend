from fastapi import FastAPI, WebSocket
from ws_handler import handle_unity_ws

app = FastAPI()

@app.get("/")
def health_check():
    return {"status": "XR backend running"}

@app.websocket("/ws")
async def websocket_endpoint(ws: WebSocket):
    await handle_unity_ws(ws)
