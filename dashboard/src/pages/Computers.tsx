import { useEffect, useState } from 'react'
import Nav from '../components/Nav'
import api from '../api'

type Computer = { id:number, identifier:string, hostname?:string, employeeId?:number }

export default function Computers() {
  const [list, setList] = useState<Computer[]>([])
  const [identifier, setIdentifier] = useState('')
  const [hostname, setHostname] = useState('')

  useEffect(() => { reload() }, [])
  function reload() { api.get('/api/admin/computers').then(r=>setList(r.data)) }

  async function add() {
    await api.post('/api/admin/computers', { identifier, hostname })
    setIdentifier(''); setHostname(''); reload()
  }

  async function del(id:number) { await api.delete(`/api/admin/computers/${id}`); reload() }

  return (
    <div>
      <Nav />
      <div className="max-w-4xl mx-auto p-4">
        <h1 className="text-2xl font-semibold mb-4">PCs</h1>
        <div className="bg-white p-3 rounded shadow mb-4 flex gap-2">
          <input placeholder="Identificador" className="border rounded px-2 py-1" value={identifier} onChange={e=>setIdentifier(e.target.value)} />
          <input placeholder="Hostname" className="border rounded px-2 py-1" value={hostname} onChange={e=>setHostname(e.target.value)} />
          <button className="bg-blue-600 text-white rounded px-3" onClick={add}>Adicionar</button>
        </div>
        <div className="bg-white rounded shadow overflow-hidden">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-gray-50 text-left">
                <th className="p-2">Identificador</th>
                <th className="p-2">Hostname</th>
                <th className="p-2">Ações</th>
              </tr>
            </thead>
            <tbody>
              {list.map(e => (
                <tr key={e.id} className="border-t">
                  <td className="p-2">{e.identifier}</td>
                  <td className="p-2">{e.hostname}</td>
                  <td className="p-2"><button className="text-red-600" onClick={()=>del(e.id)}>Remover</button></td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  )
}



