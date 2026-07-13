import { useEffect, useState } from 'react'
import './App.css'

// Esta es la "forma" que tienen los datos de un ticket, en TypeScript
interface Ticket {
  id: number
  descripcion: string
  categoria: string
  urgencia: string
  estado: string
}

function App() {
  // Aquí guardamos la lista de tickets que traemos de la API
  const [tickets, setTickets] = useState<Ticket[]>([])
  const [cargando, setCargando] = useState(true)

  // useEffect se ejecuta automáticamente cuando la página carga
  useEffect(() => {
    fetch('http://localhost:5102/api/tickets')
      .then((respuesta) => respuesta.json())
      .then((datos) => {
        setTickets(datos)
        setCargando(false)
      })
      .catch((error) => {
        console.error('Error al traer los tickets:', error)
        setCargando(false)
      })
  }, [])

  return (
    <div style={{ padding: '2rem', fontFamily: 'sans-serif' }}>
      <h1>Trivex — Tickets de Soporte</h1>

      {cargando ? (
        <p>Cargando tickets...</p>
      ) : (
        <table style={{ width: '100%', borderCollapse: 'collapse' }}>
          <thead>
            <tr style={{ borderBottom: '2px solid #ccc', textAlign: 'left' }}>
              <th>ID</th>
              <th>Descripción</th>
              <th>Categoría</th>
              <th>Urgencia</th>
              <th>Estado</th>
            </tr>
          </thead>
          <tbody>
            {tickets.map((ticket) => (
              <tr key={ticket.id} style={{ borderBottom: '1px solid #eee' }}>
                <td>{ticket.id}</td>
                <td>{ticket.descripcion}</td>
                <td>{ticket.categoria}</td>
                <td>{ticket.urgencia}</td>
                <td>{ticket.estado}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  )
}

export default App