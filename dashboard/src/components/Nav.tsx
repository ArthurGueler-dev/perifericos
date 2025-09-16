import { Link, useLocation, useNavigate } from 'react-router-dom'

export default function Nav() {
  const { pathname } = useLocation()
  const navigate = useNavigate()
  return (
    <div className="bg-white border-b">
      <div className="max-w-6xl mx-auto px-4 py-3 flex items-center justify-between">
        <div className="flex gap-4">
          <Link className={`font-semibold ${pathname==='/'?'text-blue-600':''}`} to="/">Dashboard</Link>
          <Link className={`${pathname==='/employees'?'text-blue-600':''}`} to="/employees">Colaboradores</Link>
          <Link className={`${pathname==='/computers'?'text-blue-600':''}`} to="/computers">PCs</Link>
          <Link className={`${pathname==='/peripherals'?'text-blue-600':''}`} to="/peripherals">Periféricos</Link>
          <Link className={`${pathname==='/events'?'text-blue-600':''}`} to="/events">Eventos</Link>
        </div>
        <button className="text-sm" onClick={() => { localStorage.removeItem('token'); navigate('/login') }}>Sair</button>
      </div>
    </div>
  )
}



