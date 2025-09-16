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
  useEffect(() => { reload() }, [])
  function reload() { api.get('/api/admin/events?take=500').then(r=>setEvents(r.data)) }

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
        <h1 className="text-2xl font-semibold mb-4">Eventos</h1>
        <div className="mb-3 flex gap-2">
          <button className="bg-green-600 text-white rounded px-3 py-1" onClick={exportExcel}>Exportar Excel</button>
          <button className="bg-red-600 text-white rounded px-3 py-1" onClick={exportPdf}>Exportar PDF</button>
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
              {events.map(e => (
                <tr key={e.id} className="border-t">
                  <td className="p-2 whitespace-nowrap">{new Date(e.timestampUtc).toLocaleString()}</td>
                  <td className="p-2">{e.pcIdentifier}</td>
                  <td className="p-2">{e.friendlyName}</td>
                  <td className="p-2">{e.vendorId}</td>
                  <td className="p-2">{e.productId}</td>
                  <td className="p-2">{e.serialNumber}</td>
                  <td className="p-2">{e.eventType}</td>
                  <td className="p-2">{e.isAlert? 'Sim':'Não'}</td>
                  <td className="p-2">{e.isIncident? 'Sim':'Não'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  )
}



