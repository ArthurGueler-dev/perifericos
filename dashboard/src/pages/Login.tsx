import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import api from '../api'

export default function Login() {
  const [email, setEmail] = useState('ti@example.com')
  const [password, setPassword] = useState('admin')
  const [error, setError] = useState('')
  const navigate = useNavigate()

  async function submit(e: React.FormEvent) {
    e.preventDefault()
    setError('')
    try {
      const { data } = await api.post('/api/auth/login', { email, password })
      localStorage.setItem('token', data.token)
      navigate('/')
    } catch (err) {
      setError('Falha no login')
    }
  }

  return (
    <div className="min-h-screen grid place-items-center">
      <form onSubmit={submit} className="bg-white p-6 rounded shadow w-80">
        <h1 className="text-xl font-semibold mb-4">Login - TI</h1>
        <label className="block text-sm">E-mail</label>
        <input className="border rounded w-full px-3 py-2 mb-3" value={email} onChange={e=>setEmail(e.target.value)} />
        <label className="block text-sm">Senha</label>
        <input type="password" className="border rounded w-full px-3 py-2 mb-3" value={password} onChange={e=>setPassword(e.target.value)} />
        {error && <div className="text-red-600 text-sm mb-2">{error}</div>}
        <button className="bg-blue-600 text-white rounded px-4 py-2 w-full">Entrar</button>
      </form>
    </div>
  )
}



