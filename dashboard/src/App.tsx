import { Navigate, Route, Routes } from 'react-router-dom'
import Login from './pages/Login'
import Dashboard from './pages/Dashboard'
import Employees from './pages/Employees'
import Computers from './pages/Computers'
import Peripherals from './pages/Peripherals'
import Events from './pages/Events'
import { useEffect } from 'react'

function RequireAuth({ children }: { children: JSX.Element }) {
  const token = localStorage.getItem('token')
  if (!token) return <Navigate to="/login" replace />
  return children
}

export default function App() {
  useEffect(() => {
    document.title = 'Periféricos - Dashboard'
  }, [])

  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route path="/" element={<RequireAuth><Dashboard /></RequireAuth>} />
      <Route path="/employees" element={<RequireAuth><Employees /></RequireAuth>} />
      <Route path="/computers" element={<RequireAuth><Computers /></RequireAuth>} />
      <Route path="/peripherals" element={<RequireAuth><Peripherals /></RequireAuth>} />
      <Route path="/events" element={<RequireAuth><Events /></RequireAuth>} />
      <Route path="*" element={<Navigate to="/" />} />
    </Routes>
  )
}



