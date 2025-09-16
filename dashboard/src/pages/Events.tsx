import Nav from '../components/Nav'
import { useEffect, useState } from 'react'
import api from '../api'
import { utils, writeFile } from 'xlsx'
import jsPDF from 'jspdf'

type Event = {
  id: number
  timestampUtc: string
  pcIdentifier: string
  friendlyName: string
  vendorId: string
  productId: string
  serialNumber?: string
  eventType: string
  isAlert: boolean
  isIncident: boolean
}

export default function Events() {
  const [events, setEvents] = useState<Event[]>([])
  const [filteredEvents, setFilteredEvents] = useState<Event[]>([])
  const [filterPC, setFilterPC] = useState('')
  const [filterEventType, setFilterEventType] = useState('')
  const [filterDevice, setFilterDevice] = useState('')
  const [currentPage, setCurrentPage] = useState(1)
  const [showAlertsOnly, setShowAlertsOnly] = useState(false)
  const itemsPerPage = 20

  useEffect(() => { reload() }, [])
  
  function reload() { 
    api.get('/api/admin/events?take=1000').then(r => {
      setEvents(r.data)
      setFilteredEvents(r.data)
    })
  }

  // Filtrar eventos
  useEffect(() => {
    let filtered = events.filter(event => {
      const matchPC = !filterPC || event.pcIdentifier.toLowerCase().includes(filterPC.toLowerCase())
      const matchEventType = !filterEventType || event.eventType === filterEventType
      const matchDevice = !filterDevice || event.friendlyName.toLowerCase().includes(filterDevice.toLowerCase())
      const matchAlerts = !showAlertsOnly || event.isAlert || event.isIncident
      
      return matchPC && matchEventType && matchDevice && matchAlerts
    })
    setFilteredEvents(filtered)
    setCurrentPage(1)
  }, [events, filterPC, filterEventType, filterDevice, showAlertsOnly])

  // Paginação
  const totalPages = Math.ceil(filteredEvents.length / itemsPerPage)
  const startIndex = (currentPage - 1) * itemsPerPage
  const paginatedEvents = filteredEvents.slice(startIndex, startIndex + itemsPerPage)

  async function clearAllEvents() {
    if (confirm('Tem certeza que deseja limpar TODOS os eventos? Esta ação não pode ser desfeita.')) {
      try {
        await api.delete('/api/admin/events')
        reload()
      } catch (error) {
        alert('Erro ao limpar eventos')
      }
    }
  }

  function exportExcel() {
    const ws = utils.json_to_sheet(events)
    const wb = utils.book_new()
    utils.book_append_sheet(wb, ws, 'Eventos')
    writeFile(wb, 'eventos.xlsx')
  }

  function exportPdf() {
    const doc = new jsPDF()
    doc.text('Relatório de Eventos', 10, 10)
    events.slice(0, 40).forEach((e, i) => {
      doc.text(`${new Date(e.timestampUtc).toLocaleString()} - ${e.pcIdentifier} - ${e.friendlyName} - ${e.eventType}`, 10, 20 + i * 6)
    })
    doc.save('eventos.pdf')
  }

  return (
    <div>
      <Nav />
      <div className="max-w-6xl mx-auto p-4">
        <h1 className="text-2xl font-semibold mb-4">Eventos ({filteredEvents.length} de {events.length})</h1>
        
        {/* Filtros */}
        <div className="bg-white p-4 rounded shadow mb-4">
          <div className="grid grid-cols-1 md:grid-cols-4 gap-3 mb-3">
            <input 
              placeholder="Filtrar por PC..." 
              className="border rounded px-2 py-1" 
              value={filterPC} 
              onChange={e=>setFilterPC(e.target.value)} 
            />
            <select 
              className="border rounded px-2 py-1" 
              value={filterEventType} 
              onChange={e=>setFilterEventType(e.target.value)}
            >
              <option value="">Todos os tipos</option>
              <option value="Connected">Conectado</option>
              <option value="Disconnected">Desconectado</option>
              <option value="AlertUnauthorized">Alerta</option>
            </select>
            <input 
              placeholder="Filtrar por dispositivo..." 
              className="border rounded px-2 py-1" 
              value={filterDevice} 
              onChange={e=>setFilterDevice(e.target.value)} 
            />
            <label className="flex items-center">
              <input 
                type="checkbox" 
                checked={showAlertsOnly} 
                onChange={e=>setShowAlertsOnly(e.target.checked)}
                className="mr-2"
              />
              Apenas Alertas
            </label>
          </div>
          <div className="flex gap-2">
            <button className="bg-green-600 text-white rounded px-3 py-1" onClick={exportExcel}>📊 Excel</button>
            <button className="bg-red-600 text-white rounded px-3 py-1" onClick={exportPdf}>📄 PDF</button>
            <button className="bg-blue-600 text-white rounded px-3 py-1" onClick={reload}>🔄 Atualizar</button>
            <button className="bg-orange-600 text-white rounded px-3 py-1" onClick={clearAllEvents}>🗑️ Limpar Todos</button>
          </div>
        </div>
        <div className="bg-white rounded shadow overflow-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-gray-50 text-left">
                <th className="p-2">Data/Hora</th>
                <th className="p-2">PC</th>
                <th className="p-2">Dispositivo</th>
                <th className="p-2">VID</th>
                <th className="p-2">PID</th>
                <th className="p-2">Serial</th>
                <th className="p-2">Evento</th>
                <th className="p-2">Alerta</th>
                <th className="p-2">Incidente</th>
              </tr>
            </thead>
            <tbody>
              {paginatedEvents.map(e => (
                <tr key={e.id} className={`border-t ${e.isAlert || e.isIncident ? 'bg-red-50' : ''}`}>
                  <td className="p-2 whitespace-nowrap">{new Date(e.timestampUtc).toLocaleString()}</td>
                  <td className="p-2 font-medium">{e.pcIdentifier}</td>
                  <td className="p-2">{e.friendlyName}</td>
                  <td className="p-2 font-mono text-xs">{e.vendorId}</td>
                  <td className="p-2 font-mono text-xs">{e.productId}</td>
                  <td className="p-2 font-mono text-xs">{e.serialNumber || '-'}</td>
                  <td className="p-2">
                    <span className={`px-2 py-1 rounded text-xs ${
                      e.eventType === 'Connected' ? 'bg-green-100 text-green-800' :
                      e.eventType === 'Disconnected' ? 'bg-gray-100 text-gray-800' :
                      'bg-red-100 text-red-800'
                    }`}>
                      {e.eventType}
                    </span>
                  </td>
                  <td className="p-2">{e.isAlert ? '🚨' : '✅'}</td>
                  <td className="p-2">{e.isIncident ? '⚠️' : '✅'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {/* Paginação */}
        {totalPages > 1 && (
          <div className="mt-4 flex justify-center items-center gap-2">
            <button 
              onClick={() => setCurrentPage(Math.max(1, currentPage - 1))}
              disabled={currentPage === 1}
              className="px-3 py-1 rounded bg-gray-200 hover:bg-gray-300 disabled:opacity-50"
            >
              ← Anterior
            </button>
            
            <span className="mx-4">
              Página {currentPage} de {totalPages}
            </span>
            
            <button 
              onClick={() => setCurrentPage(Math.min(totalPages, currentPage + 1))}
              disabled={currentPage === totalPages}
              className="px-3 py-1 rounded bg-gray-200 hover:bg-gray-300 disabled:opacity-50"
            >
              Próxima →
            </button>
          </div>
        )}
      </div>
    </div>
  )
}



