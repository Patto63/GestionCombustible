import { useEffect, useState } from 'react';
import { apiFetch } from '../api/base';

interface UsuarioDto {
  usuarioId: number;
  nombre: string;
  apellido: string;
  correo: string;
  nombreUsuario: string;
  estaActivo: boolean;
  roles: string[];
}

export const UserManagement = () => {
  const [usuarios, setUsuarios] = useState<UsuarioDto[]>([]);

  useEffect(() => {
    apiFetch<{ usuarios: UsuarioDto[] }>('auth/usuarios')
      .then(r => setUsuarios(r.usuarios))
      .catch(err => console.error(err));
  }, []);

  return (
    <div>
      <h2>Usuarios</h2>
      <ul>
        {usuarios.map(u => (
          <li key={u.usuarioId}>{u.nombre} {u.apellido} - {u.roles.join(', ')}</li>
        ))}
      </ul>
    </div>
  );
};
