import { Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

export const Navbar = () => {
  const { roles, logout } = useAuth();
  const isAdmin = roles.includes('Administrador');

  return (
    <nav>
      <Link to="/">Inicio</Link>
      {' | '}
      <Link to="/vehicles">Vehículos</Link>
      {' | '}
      {isAdmin && <Link to="/users">Usuarios</Link>}
      {' | '}
      <button onClick={logout}>Salir</button>
    </nav>
  );
};
