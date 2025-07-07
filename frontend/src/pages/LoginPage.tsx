import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { login } from '../api/auth';
import { useAuth } from '../contexts/AuthContext';

export const LoginPage = () => {
  const navigate = useNavigate();
  const { login: saveToken } = useAuth();
  const [correo, setCorreo] = useState('');
  const [contrasena, setContrasena] = useState('');
  const [error, setError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const res = await login({ correo, contrasena });
      if (res.exito) {
        saveToken(res.token);
        navigate('/');
      } else {
        setError(res.mensaje);
      }
    } catch (err) {
      setError('Error al iniciar sesión');
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <div>
        <label>Correo</label>
        <input value={correo} onChange={e => setCorreo(e.target.value)} />
      </div>
      <div>
        <label>Contraseña</label>
        <input type="password" value={contrasena} onChange={e => setContrasena(e.target.value)} />
      </div>
      {error && <p style={{ color: 'red' }}>{error}</p>}
      <button type="submit">Entrar</button>
    </form>
  );
};
