import { useEffect, useState } from 'react'
import './App.css'

interface Ticket {
  id: number
  descripcion: string
  categoria: string
  urgencia: string
  estado: string
}

function App() {
  const [tickets, setTickets] = useState<Ticket[]>([])
  const [cargando, setCargando] = useState(true)

  // Estos son los "campos" del formulario, cada uno con su propia cajita de estado
  const [descripcion, setDescripcion] = useState('')
  const [categoria, setCategoria] = useState('')
  const [urgencia, setUrgencia] = useState('Baja')
  const [enviando, setEnviando] = useState(false)

  // Función para traer los tickets de la API (la separamos para poder reutilizarla)
  const cargarTickets = () => {
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
  }

  useEffect(() => {
    cargarTickets()
  }, [])

  // Función que se ejecuta cuando el usuario envía el formulario
  const manejarEnvio = async (evento: React.FormEvent) => {
    evento.preventDefault() // evita que la página se recargue (comportamiento normal de un form)

    if (!descripcion || !categoria) {
      alert('Por favor llena descripción y categoría')
      return
    }

    setEnviando(true)

    try {
      const respuesta = await fetch('http://localhost:5102/api/tickets', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          descripcion,
          categoria,
          urgencia,
          estado: 'Pendiente',
        }),
      })

      if (respuesta.ok) {
        // Limpiamos el formulario
        setDescripcion('')
        setCategoria('')
        setUrgencia('Baja')
        // Volvemos a cargar la lista para que aparezca el ticket nuevo
        cargarTickets()
      } else {
        alert('Hubo un error al crear el ticket')
      }
    } catch (error) {
      console.error('Error:', error)
      alert('No se pudo conectar con el servidor')
    } finally {
      setEnviando(false)
    }
  }

  return (
    <div style={{ padding: '2rem', fontFamily: 'sans-serif', maxWidth: '900px', margin: '0 auto' }}>
      <h1>Trivex — Tickets de Soporte</h1>

      {/* Formulario para crear tickets nuevos */}
      <form onSubmit={manejarEnvio} style={{ marginBottom: '2rem', padding: '1rem', border: '1px solid #444', borderRadius: '8px' }}>
        <h2 style={{ fontSize: '1.2rem' }}>Crear nuevo ticket</h2>

        <div style={{ marginBottom: '0.75rem' }}>
          <label>Descripción: </label>
          <input
            type="text"
            value={descripcion}
            onChange={(e) => setDescripcion(e.target.value)}
            placeholder="Ej: No puedo pagar con tarjeta"
            style={{ width: '100%', padding: '0.5rem', marginTop: '0.25rem' }}
          />
        </div>

        <div style={{ marginBottom: '0.75rem' }}>
          <label>Categoría: </label>
          <input
            type="text"
            value={categoria}
            onChange={(e) => setCategoria(e.target.value)}
            placeholder="Ej: Pagos, Acceso, Bug..."
            style={{ width: '100%', padding: '0.5rem', marginTop: '0.25rem' }}
          />
        </div>

        <div style={{ marginBottom: '0.75rem' }}>
          <label>Urgencia: </label>
          <select
            value={urgencia}
            onChange={(e) => setUrgencia(e.target.value)}
            style={{ width: '100%', padding: '0.5rem', marginTop: '0.25rem' }}
          >
            <option value="Baja">Baja</option>
            <option value="Media">Media</option>
            <option value="Alta">Alta</option>
          </select>
        </div>

        <button type="submit" disabled={enviando} style={{ padding: '0.5rem 1.5rem' }}>
          {enviando ? 'Creando...' : 'Crear Ticket'}
        </button>
      </form>

      {/* Tabla de tickets */}
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