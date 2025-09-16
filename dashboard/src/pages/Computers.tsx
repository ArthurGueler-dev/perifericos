import { useEffect, useState } from 'react'
import Nav from '../components/Nav'
import api from '../api'

type Computer = { id:number, identifier:string, hostname?:string, employeeId?:number }
type Peripheral = { id:number, vendorId:string, productId:string, serialNumber?:string, friendlyName:string }
type Employee = { id:number, name:string, email:string }

export default function Computers() {
  const [list, setList] = useState<Computer[]>([])
  const [identifier, setIdentifier] = useState('')
  const [hostname, setHostname] = useState('')
  const [peripherals, setPeripherals] = useState<Peripheral[]>([])
  const [employees, setEmployees] = useState<Employee[]>([])
  const [selectedComputer, setSelectedComputer] = useState<Computer | null>(null)
  const [selectedPeripheral, setSelectedPeripheral] = useState('')
  const [selectedEmployee, setSelectedEmployee] = useState('')

  useEffect(() => { reload(); loadPeripherals(); loadEmployees() }, [])
  function reload() { api.get('/api/admin/computers').then(r=>setList(r.data)) }
  function loadPeripherals() { api.get('/api/admin/peripherals').then(r=>setPeripherals(r.data)) }
  function loadEmployees() { api.get('/api/admin/employees').then(r=>setEmployees(r.data)) }

  async function add() {
    await api.post('/api/admin/computers', { identifier, hostname })
    setIdentifier(''); setHostname(''); reload()
  }

  async function del(id:number) { await api.delete(`/api/admin/computers/${id}`); reload() }

  async function assignPeripheral() {
    if (!selectedComputer || !selectedPeripheral) return
    await api.post('/api/admin/assignments', { 
      peripheralId: parseInt(selectedPeripheral), 
      computerId: selectedComputer.id 
    })
    setSelectedComputer(null); setSelectedPeripheral(''); reload()
  }

  async function assignEmployee() {
    if (!selectedComputer || !selectedEmployee) return
    await api.put(`/api/admin/computers/${selectedComputer.id}`, { 
      ...selectedComputer, 
      employeeId: parseInt(selectedEmployee) 
    })
    setSelectedComputer(null); setSelectedEmployee(''); reload()
  }

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
                <th className="p-2">Colaborador</th>
                <th className="p-2">Ações</th>
              </tr>
            </thead>
            <tbody>
              {list.map(e => (
                <tr key={e.id} className="border-t">
                  <td className="p-2">{e.identifier}</td>
                  <td className="p-2">{e.hostname}</td>
                  <td className="p-2">{employees.find(emp => emp.id === e.employeeId)?.name || '-'}</td>
                  <td className="p-2 space-x-2">
                    <button className="text-blue-600" onClick={()=>setSelectedComputer(e)}>Configurar</button>
                    <button className="text-red-600" onClick={()=>del(e.id)}>Remover</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {/* Modal de Configuração */}
        {selectedComputer && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center">
            <div className="bg-white rounded-lg p-6 w-96">
              <h2 className="text-xl font-semibold mb-4">Configurar {selectedComputer.identifier}</h2>
              
              {/* Vincular Colaborador */}
              <div className="mb-4">
                <label className="block text-sm font-medium mb-2">Vincular Colaborador:</label>
                <div className="flex gap-2">
                  <select 
                    className="border rounded px-2 py-1 flex-1" 
                    value={selectedEmployee} 
                    onChange={e=>setSelectedEmployee(e.target.value)}
                  >
                    <option value="">Selecione um colaborador</option>
                    {employees.map(emp => (
                      <option key={emp.id} value={emp.id}>{emp.name}</option>
                    ))}
                  </select>
                  <button 
                    className="bg-green-600 text-white px-3 py-1 rounded"
                    onClick={assignEmployee}
                    disabled={!selectedEmployee}
                  >
                    Vincular
                  </button>
                </div>
              </div>

              {/* Vincular Periférico */}
              <div className="mb-4">
                <label className="block text-sm font-medium mb-2">Vincular Periférico:</label>
                <div className="flex gap-2">
                  <select 
                    className="border rounded px-2 py-1 flex-1" 
                    value={selectedPeripheral} 
                    onChange={e=>setSelectedPeripheral(e.target.value)}
                  >
                    <option value="">Selecione um periférico</option>
                    {peripherals.map(per => (
                      <option key={per.id} value={per.id}>{per.friendlyName}</option>
                    ))}
                  </select>
                  <button 
                    className="bg-blue-600 text-white px-3 py-1 rounded"
                    onClick={assignPeripheral}
                    disabled={!selectedPeripheral}
                  >
                    Vincular
                  </button>
                </div>
              </div>

              <div className="flex justify-end">
                <button 
                  className="bg-gray-600 text-white px-4 py-2 rounded"
                  onClick={()=>setSelectedComputer(null)}
                >
                  Fechar
                </button>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  )
}



