import { useEffect, useState } from 'react'
import Nav from '../components/Nav'
import api from '../api'

type Employee = { id:number, name:string, email:string }

export default function Employees() {
  const [list, setList] = useState<Employee[]>([])
  const [name, setName] = useState('')
  const [email, setEmail] = useState('')

  useEffect(() => { reload() }, [])
  function reload() { api.get('/api/admin/employees').then(r=>setList(r.data)) }

  async function add() {
    await api.post('/api/admin/employees', { name, email })
    setName(''); setEmail(''); reload()
  }

  async function del(id:number) { await api.delete(`/api/admin/employees/${id}`); reload() }

  return (
    <div>
      <Nav />
      <div className="max-w-4xl mx-auto p-4">
        <h1 className="text-2xl font-semibold mb-4">Colaboradores</h1>
        <div className="bg-white p-3 rounded shadow mb-4 flex gap-2">
          <input placeholder="Nome" className="border rounded px-2 py-1" value={name} onChange={e=>setName(e.target.value)} />
          <input placeholder="Email" className="border rounded px-2 py-1" value={email} onChange={e=>setEmail(e.target.value)} />
          <button className="bg-blue-600 text-white rounded px-3" onClick={add}>Adicionar</button>
        </div>
        <div className="bg-white rounded shadow overflow-hidden">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-gray-50 text-left">
                <th className="p-2">Nome</th>
                <th className="p-2">Email</th>
                <th className="p-2">Ações</th>
              </tr>
            </thead>
            <tbody>
              {list.map(e => (
                <tr key={e.id} className="border-t">
                  <td className="p-2">{e.name}</td>
                  <td className="p-2">{e.email}</td>
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



