import Nav from '../components/Nav'
import { useEffect, useMemo, useState } from 'react'
import api from '../api'
import * as signalR from '@microsoft/signalr'

type Event = {
  id: number
  timestampUtc: string
  pcIdentifier: string
  friendlyName: string
  serialNumber?: string
  eventType: string
}

export default function Dashboard() {
  const [events, setEvents] = useState<Event[]>([])
  const [connected, setConnected] = useState(false)

  useEffect(() => {
    api.get('/api/admin/events?take=20').then(r=>setEvents(r.data))
  }, [])

  const hubUrl = useMemo(() => {
    const base = import.meta.env.VITE_API_BASE ?? 'https://api.in9automacao.com.br'
    const token = localStorage.getItem('token')
    return `${base}/hubs/alerts?access_token=${token}`
  }, [])

  useEffect(() => {
    const connection = new signalR.HubConnectionBuilder().withUrl(hubUrl).withAutomaticReconnect().build()
    connection.on('alert', (msg:any) => {
      setEvents(prev => [{
        id: msg.id, timestampUtc: msg.timestampUtc, pcIdentifier: msg.pcIdentifier,
        friendlyName: msg.friendlyName, serialNumber: msg.serialNumber, eventType: msg.eventType
      }, ...prev])
      // toast simples
      // eslint-disable-next-line no-alert
      alert(`Alerta: ${msg.friendlyName} em ${msg.pcIdentifier}`)
    })
    connection.start().then(()=>setConnected(true)).catch(()=>setConnected(false))
    return () => { connection.stop() }
  }, [hubUrl])

  return (
    <div>
      <Nav />
      <div className="max-w-6xl mx-auto p-4">
        <h1 className="text-2xl font-semibold mb-4">Visão Geral</h1>
        <div className="text-sm text-gray-600 mb-2">SignalR: {connected? 'conectado' : 'desconectado'}</div>
        <div className="bg-white rounded shadow">
          <div className="p-3 border-b font-medium">Últimos alertas/eventos</div>
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-gray-50 text-left">
                <th className="p-2">Data/Hora</th>
                <th className="p-2">PC</th>
                <th className="p-2">Dispositivo</th>
                <th className="p-2">Evento</th>
              </tr>
            </thead>
            <tbody>
              {events.map(e => (
                <tr key={e.id} className="border-t">
                  <td className="p-2">{new Date(e.timestampUtc).toLocaleString()}</td>
                  <td className="p-2">{e.pcIdentifier}</td>
                  <td className="p-2">{e.friendlyName} {e.serialNumber? `(${e.serialNumber})` : ''}</td>
                  <td className="p-2">{e.eventType}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  )
}



