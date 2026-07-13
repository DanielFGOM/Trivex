from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from groq import Groq
from dotenv import load_dotenv
import os
import json

load_dotenv()

cliente = Groq(api_key=os.getenv("GROQ_API_KEY"))

app = FastAPI()

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"],
)


class TicketParaAnalizar(BaseModel):
    descripcion: str


@app.post("/analizar")
def analizar_ticket(ticket: TicketParaAnalizar):
    respuesta = cliente.chat.completions.create(
        model="llama-3.3-70b-versatile",
        messages=[
            {
                "role": "system",
                "content": (
                    "Eres un asistente que clasifica tickets de soporte al cliente. "
                    "Dada la descripción de una queja, responde SOLO con un JSON que tenga "
                    "estas 2 propiedades: 'categoria' (una palabra corta, ej: Pagos, Acceso, Bug, Sugerencia) "
                    "y 'urgencia' (debe ser exactamente 'Baja', 'Media' o 'Alta'). "
                    "No agregues texto extra, solo el JSON."
                ),
            },
            {
                "role": "user",
                "content": ticket.descripcion,
            },
        ],
        response_format={"type": "json_object"},
    )

    contenido = respuesta.choices[0].message.content
    resultado = json.loads(contenido)

    return resultado