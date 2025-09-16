import { useEffect, useState } from 'react'
import Nav from '../components/Nav'
import api from '../api'

type Peripheral = { id:number, vendorId:string, productId:string, serialNumber?:string, friendlyName:string }

export default function Peripherals() {
  const [list, setList] = useState<Peripheral[]>([])
  const [friendlyName, setFriendlyName] = useState('')
  const [vendorId, setVendorId] = useState('')
  const [productId, setProductId] = useState('')
  const [serialNumber, setSerialNumber] = useState('')

  useEffect(() => { reload() }, [])
  function reload() { api.get('/api/admin/peripherals').then(r=>setList(r.data)) }

  async function add() {
    await api.post('/api/admin/peripherals', { friendlyName, vendorId, productId, serialNumber })
    setFriendlyName(''); setVendorId(''); setProductId(''); setSerialNumber(''); reload()
  }

  async function del(id:number) { await api.delete(`/api/admin/peripherals/${id}`); reload() }

  return (
    <div>
      <Nav />
      <div className="max-w-4xl mx-auto p-4">
        <h1 className="text-2xl font-semibold mb-4">Periféricos</h1>
        <div className="bg-white p-3 rounded shadow mb-4 grid grid-cols-5 gap-2">
          <input placeholder="Nome" className="border rounded px-2 py-1" value={friendlyName} onChange={e=>setFriendlyName(e.target.value)} />
          <input placeholder="VendorId (VID)" className="border rounded px-2 py-1" value={vendorId} onChange={e=>setVendorId(e.target.value)} />
          <input placeholder="ProductId (PID)" className="border rounded px-2 py-1" value={productId} onChange={e=>setProductId(e.target.value)} />
          <input placeholder="Serial" className="border rounded px-2 py-1" value={serialNumber} onChange={e=>setSerialNumber(e.target.value)} />
          <button className="bg-blue-600 text-white rounded px-3" onClick={add}>Adicionar</button>
        </div>
        <div className="bg-white rounded shadow overflow-hidden">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-gray-50 text-left">
                <th className="p-2">Nome</th>
                <th className="p-2">VID</th>
                <th className="p-2">PID</th>
                <th className="p-2">Serial</th>
                <th className="p-2">Ações</th>
              </tr>
            </thead>
            <tbody>
              {list.map(e => (
                <tr key={e.id} className="border-t">
                  <td className="p-2">{e.friendlyName}</td>
                  <td className="p-2">{e.vendorId}</td>
                  <td className="p-2">{e.productId}</td>
                  <td className="p-2">{e.serialNumber}</td>
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



